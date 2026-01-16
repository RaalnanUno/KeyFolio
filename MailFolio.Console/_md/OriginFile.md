Awesome — here’s the **full, paste-ready file set** in the exact “file path → paste this” format.

Key changes you requested are built in:

* ✅ **Silent runs**: no logs during execution
* ✅ **Only outputs** `True` or `False`
* ✅ **Only writes to SQLite on failure**
* ✅ **Limited error codes**
* ✅ **Upsert by error code**: if the same error code happens again, we **update the existing record** (most recent date + payload + count), instead of inserting a new row
* ✅ **Landmine-proof** env var handling: missing env vars become a specific error code + saved failure payload

---

# What users must set (the “landmine” explanation)

MailFolio won’t “guess” secrets. It needs **two environment variables** to run:

### Required environment variables

1. **`MAILFOLIO_PASSPHRASE`**
   The passphrase used by **KeyFolio** to decrypt the encrypted values in the KeyFile JSON.

2. **`MAILFOLIO_KEYFILE`**
   Full path to the KeyFile JSON containing SMTP settings + encrypted username/password.

### Optional

* **`MAILFOLIO_DB`**
  Path to SQLite DB file. If not set, defaults to:
  `.\Data\MailFolio.db` (relative to the exe folder)

### What happens if they forget?

* MailFolio will:

  * print only `False`
  * exit code `1`
  * write/update a failure record in SQLite under error code:

    * `MissingEnvVar`

So: even with missing env vars, you still get a record of “what went wrong” without console spam.

---

# Exit codes (limited + stable)

We keep exit codes very simple for schedulers:

* **0** = success (`True`)
* **1** = failure (`False`)

The **detailed failure reason** is stored in SQLite by **ErrorCode** and JSON payload.

---

# Project folder layout

```
MailFolio.Console/
  Program.cs
  schema.sql
  Cli/
    MailFolioArgs.cs
    MailFolioArgsParser.cs
  Config/
    MailFolioKeyFile.cs
    KeyFileLoader.cs
  Crypto/
    EnvSecretProvider.cs
  Data/
    SentMailRepository.cs
  Mail/
    MailSender.cs
    TlsMode.cs
  Reporting/
    ErrorCodes.cs
    FailureReport.cs
```

---

# 1) `MailFolio.Console/schema.sql`  → paste this

```sql
-- Failure-only ledger, aggregated by error code.
-- We only store ONE row per ErrorCode and keep the most recent payload.

CREATE TABLE IF NOT EXISTS SentMail (
    ErrorCode           TEXT    NOT NULL PRIMARY KEY, -- one row per code
    LastOccurredUtc     TEXT    NOT NULL,              -- ISO-8601 UTC timestamp
    OccurrenceCount     INTEGER NOT NULL,

    -- Useful context (safe-ish, optional, no subject/body)
    FromEmail           TEXT    NULL,
    ToEmail             TEXT    NULL,
    Server              TEXT    NULL,
    Port                INTEGER NULL,
    TlsMode             TEXT    NULL,
    DryRun              INTEGER NOT NULL,

    -- UTF-8 JSON (sanitized failure report)
    ResultJson          BLOB    NOT NULL
);

CREATE INDEX IF NOT EXISTS IX_SentMail_LastOccurredUtc ON SentMail(LastOccurredUtc);
```

> Note: This intentionally becomes an **error ledger** (not a sent-mail history).
> It matches your “don’t store message info on success” + “update record if error code already exists” requirements.

---

# 2) `MailFolio.Console/Reporting/ErrorCodes.cs` → paste this

```csharp
namespace MailFolio.Console.Reporting;

public static class ErrorCodes
{
    // Environment & configuration
    public const string MissingEnvVar = "MissingEnvVar";
    public const string InvalidKeyFile = "InvalidKeyFile";
    public const string MissingKeyFile = "MissingKeyFile";
    public const string KeyFolioDecryptFailed = "KeyFolioDecryptFailed";

    // CLI / user input
    public const string InvalidArgs = "InvalidArgs";
    public const string MissingBody = "MissingBody";
    public const string BodyFileNotFound = "BodyFileNotFound";
    public const string AttachmentNotFound = "AttachmentNotFound";
    public const string InvalidEmailAddress = "InvalidEmailAddress";

    // SMTP / mail sending
    public const string SmtpConnectFailed = "SmtpConnectFailed";
    public const string SmtpAuthFailed = "SmtpAuthFailed";
    public const string SmtpSendFailed = "SmtpSendFailed";

    // Database
    public const string DbWriteFailed = "DbWriteFailed";

    // Catch-all
    public const string Unknown = "Unknown";
}
```

---

# 3) `MailFolio.Console/Reporting/FailureReport.cs` → paste this

```csharp
using System.Text.Json.Serialization;

namespace MailFolio.Console.Reporting;

public sealed record FailureReport
{
    public string ErrorCode { get; init; } = ErrorCodes.Unknown;

    public DateTimeOffset StartedUtc { get; init; }
    public DateTimeOffset FinishedUtc { get; init; }
    public long DurationMs { get; init; }

    public bool DryRun { get; init; }

    // Safe context (no subject/body, no username/password)
    public string? FromEmail { get; init; }
    public string? ToEmail { get; init; }
    public string? Server { get; init; }
    public int? Port { get; init; }
    public string? TlsMode { get; init; }

    public string? BodySource { get; init; } // "Body" | "BodyFile"
    public bool? IsHtml { get; init; }

    public string[] Attachments { get; init; } = Array.Empty<string>();

    public ErrorInfo Error { get; init; } = new();

    public sealed record ErrorInfo
    {
        public string Type { get; init; } = "";
        public string Message { get; init; } = "";
        public string? StackTrace { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ErrorInfo? Inner { get; init; }
    }
}
```

---

# 4) `MailFolio.Console/Crypto/EnvSecretProvider.cs` → paste this

```csharp
using KeyFolio.Core.Crypto;

namespace MailFolio.Console.Crypto;

public sealed class EnvSecretProvider : ISecretProvider
{
    private readonly string _envVar;

    public EnvSecretProvider(string envVar)
    {
        _envVar = envVar ?? throw new ArgumentNullException(nameof(envVar));
    }

    public string GetSecret()
    {
        var v = Environment.GetEnvironmentVariable(_envVar);
        if (string.IsNullOrWhiteSpace(v))
            throw new InvalidOperationException($"Missing required environment variable '{_envVar}'.");
        return v;
    }
}
```

---

# 5) `MailFolio.Console/Config/MailFolioKeyFile.cs` → paste this

```csharp
namespace MailFolio.Console.Config;

public sealed record MailFolioKeyFile
{
    public string Server { get; init; } = "";
    public int Port { get; init; } = 587;

    // "StartTls" (recommended), "SslOnConnect", or "None"
    public string TlsMode { get; init; } = "StartTls";

    public string FromEmail { get; init; } = "";
    public string FromName { get; init; } = "";

    // KeyFolio envelope strings (encrypted)
    public string UsernameEnc { get; init; } = "";
    public string PasswordEnc { get; init; } = "";
}
```

---

# 6) `MailFolio.Console/Config/KeyFileLoader.cs` → paste this

```csharp
using System.Text.Json;
using MailFolio.Console.Reporting;

namespace MailFolio.Console.Config;

public static class KeyFileLoader
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static MailFolioKeyFile Load(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new MailFolioException(ErrorCodes.MissingKeyFile, "KeyFile path was empty.");

        if (!File.Exists(path))
            throw new MailFolioException(ErrorCodes.MissingKeyFile, $"KeyFile not found: {path}");

        try
        {
            var json = File.ReadAllText(path);
            var obj = JsonSerializer.Deserialize<MailFolioKeyFile>(json, JsonOpts);

            if (obj is null)
                throw new MailFolioException(ErrorCodes.InvalidKeyFile, "KeyFile JSON parsed to null.");

            // Minimal validation (no secrets revealed)
            if (string.IsNullOrWhiteSpace(obj.Server)) throw new MailFolioException(ErrorCodes.InvalidKeyFile, "KeyFile missing Server.");
            if (obj.Port <= 0) throw new MailFolioException(ErrorCodes.InvalidKeyFile, "KeyFile Port must be > 0.");
            if (string.IsNullOrWhiteSpace(obj.FromEmail)) throw new MailFolioException(ErrorCodes.InvalidKeyFile, "KeyFile missing FromEmail.");
            if (string.IsNullOrWhiteSpace(obj.UsernameEnc)) throw new MailFolioException(ErrorCodes.InvalidKeyFile, "KeyFile missing UsernameEnc.");
            if (string.IsNullOrWhiteSpace(obj.PasswordEnc)) throw new MailFolioException(ErrorCodes.InvalidKeyFile, "KeyFile missing PasswordEnc.");

            return obj;
        }
        catch (MailFolioException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new MailFolioException(ErrorCodes.InvalidKeyFile, "Failed to parse KeyFile JSON.", ex);
        }
    }
}
```

---

# 7) `MailFolio.Console/Reporting/MailFolioException.cs` → paste this

```csharp
namespace MailFolio.Console.Reporting;

public sealed class MailFolioException : Exception
{
    public string ErrorCode { get; }

    public MailFolioException(string errorCode, string message, Exception? inner = null)
        : base(message, inner)
    {
        ErrorCode = errorCode;
    }
}
```

---

# 8) `MailFolio.Console/Cli/MailFolioArgs.cs` → paste this

```csharp
namespace MailFolio.Console.Cli;

public sealed class MailFolioArgs
{
    public string ToEmail { get; set; } = "";

    public string Subject { get; set; } = ""; // used for sending, but never stored in DB
    public string? Body { get; set; }
    public string? BodyFile { get; set; }

    public bool DryRun { get; set; }
    public bool HtmlFlag { get; set; } // optional override if you later want it
    public bool Verbose { get; set; }  // parsed but not used for console logs (kept for future)

    public List<string> Attachments { get; } = new();
    public List<string> Cc { get; } = new();
    public List<string> Bcc { get; } = new();
    public List<string> ReplyTo { get; } = new();
}
```

---

# 9) `MailFolio.Console/Cli/MailFolioArgsParser.cs` → paste this

```csharp
using System.Net.Mail;
using MailFolio.Console.Reporting;

namespace MailFolio.Console.Cli;

public static class MailFolioArgsParser
{
    public static MailFolioArgs Parse(string[] args)
    {
        // Very small parser: --key value, flags like --dryRun
        var a = new MailFolioArgs();
        var i = 0;

        while (i < args.Length)
        {
            var token = args[i];

            if (!token.StartsWith("--", StringComparison.Ordinal))
                throw new MailFolioException(ErrorCodes.InvalidArgs, $"Unexpected token '{token}'. Expected '--option'.");

            var key = token[2..];

            // Flags
            if (IsFlag(key))
            {
                ApplyFlag(a, key);
                i++;
                continue;
            }

            // Key/value
            if (i + 1 >= args.Length)
                throw new MailFolioException(ErrorCodes.InvalidArgs, $"Missing value for '--{key}'.");

            var value = args[i + 1];
            ApplyKeyValue(a, key, value);
            i += 2;
        }

        // Validate required
        if (string.IsNullOrWhiteSpace(a.ToEmail))
            throw new MailFolioException(ErrorCodes.InvalidArgs, "Missing required '--to'.");

        ValidateEmail(a.ToEmail, ErrorCodes.InvalidEmailAddress);

        foreach (var cc in a.Cc) ValidateEmail(cc, ErrorCodes.InvalidEmailAddress);
        foreach (var bcc in a.Bcc) ValidateEmail(bcc, ErrorCodes.InvalidEmailAddress);
        foreach (var rt in a.ReplyTo) ValidateEmail(rt, ErrorCodes.InvalidEmailAddress);

        // Body rules:
        // - Default: plain text with --body
        // - If --bodyFile provided: assume HTML
        // - Require one of them
        if (string.IsNullOrWhiteSpace(a.BodyFile) && string.IsNullOrWhiteSpace(a.Body))
            throw new MailFolioException(ErrorCodes.MissingBody, "Either '--body' or '--bodyFile' must be provided.");

        if (!string.IsNullOrWhiteSpace(a.BodyFile) && !File.Exists(a.BodyFile))
            throw new MailFolioException(ErrorCodes.BodyFileNotFound, $"BodyFile not found: {a.BodyFile}");

        // Attachments existence
        foreach (var path in a.Attachments)
        {
            if (!File.Exists(path))
                throw new MailFolioException(ErrorCodes.AttachmentNotFound, $"Attachment not found: {path}");
        }

        return a;
    }

    private static bool IsFlag(string key) =>
        key.Equals("dryRun", StringComparison.OrdinalIgnoreCase) ||
        key.Equals("html", StringComparison.OrdinalIgnoreCase) ||
        key.Equals("verbose", StringComparison.OrdinalIgnoreCase);

    private static void ApplyFlag(MailFolioArgs a, string key)
    {
        if (key.Equals("dryRun", StringComparison.OrdinalIgnoreCase)) a.DryRun = true;
        else if (key.Equals("html", StringComparison.OrdinalIgnoreCase)) a.HtmlFlag = true;
        else if (key.Equals("verbose", StringComparison.OrdinalIgnoreCase)) a.Verbose = true;
        else throw new MailFolioException(ErrorCodes.InvalidArgs, $"Unknown flag '--{key}'.");
    }

    private static void ApplyKeyValue(MailFolioArgs a, string key, string value)
    {
        switch (key.ToLowerInvariant())
        {
            case "to":
                a.ToEmail = value;
                break;

            case "subject":
                a.Subject = value ?? "";
                break;

            case "body":
                a.Body = value;
                break;

            case "bodyfile":
                a.BodyFile = value;
                break;

            case "attach":
                a.Attachments.Add(value);
                break;

            case "cc":
                a.Cc.Add(value);
                break;

            case "bcc":
                a.Bcc.Add(value);
                break;

            case "replyto":
                a.ReplyTo.Add(value);
                break;

            default:
                throw new MailFolioException(ErrorCodes.InvalidArgs, $"Unknown option '--{key}'.");
        }
    }

    private static void ValidateEmail(string email, string errorCode)
    {
        try
        {
            _ = new MailAddress(email);
        }
        catch
        {
            throw new MailFolioException(errorCode, $"Invalid email address: {email}");
        }
    }
}
```

---

# 10) `MailFolio.Console/Mail/TlsMode.cs` → paste this

```csharp
namespace MailFolio.Console.Mail;

public enum TlsMode
{
    None,
    StartTls,
    SslOnConnect
}

public static class TlsModeParser
{
    public static TlsMode Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return TlsMode.StartTls;

        return value.Trim().ToLowerInvariant() switch
        {
            "none" => TlsMode.None,
            "starttls" => TlsMode.StartTls,
            "sslonconnect" => TlsMode.SslOnConnect,
            _ => TlsMode.StartTls
        };
    }
}
```

---

# 11) `MailFolio.Console/Mail/MailSender.cs` → paste this

> NuGet needed:
>
> * `MailKit`

```csharp
using MailFolio.Console.Cli;
using MailFolio.Console.Config;
using MailFolio.Console.Reporting;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace MailFolio.Console.Mail;

public static class MailSender
{
    public sealed record ResolvedBody(string Source, bool IsHtml, string Content);

    public static ResolvedBody ResolveBody(MailFolioArgs args)
    {
        // If BodyFile specified: assume HTML
        if (!string.IsNullOrWhiteSpace(args.BodyFile))
        {
            var html = File.ReadAllText(args.BodyFile);
            return new ResolvedBody(Source: "BodyFile", IsHtml: true, Content: html);
        }

        // Default: plain text body
        if (!string.IsNullOrWhiteSpace(args.Body))
            return new ResolvedBody(Source: "Body", IsHtml: false, Content: args.Body);

        // Parser should prevent this, but keep it defensive
        throw new MailFolioException(ErrorCodes.MissingBody, "Either '--body' or '--bodyFile' must be provided.");
    }

    public static string? Send(
        MailFolioKeyFile keyFile,
        string smtpUsername,
        string smtpPassword,
        MailFolioArgs args,
        ResolvedBody body)
    {
        var message = BuildMessage(keyFile, args, body);

        var tlsMode = TlsModeParser.Parse(keyFile.TlsMode);
        var socketOpt = tlsMode switch
        {
            TlsMode.None => SecureSocketOptions.None,
            TlsMode.SslOnConnect => SecureSocketOptions.SslOnConnect,
            _ => SecureSocketOptions.StartTls
        };

        try
        {
            using var client = new SmtpClient();

            try
            {
                client.Connect(keyFile.Server, keyFile.Port, socketOpt);
            }
            catch (Exception ex)
            {
                throw new MailFolioException(ErrorCodes.SmtpConnectFailed, "Failed to connect to SMTP server.", ex);
            }

            try
            {
                client.Authenticate(smtpUsername, smtpPassword);
            }
            catch (Exception ex)
            {
                throw new MailFolioException(ErrorCodes.SmtpAuthFailed, "SMTP authentication failed.", ex);
            }

            try
            {
                // Returns server response string. Message-Id is in message.MessageId.
                client.Send(message);
                client.Disconnect(true);

                // Return a provider-ish identifier if useful
                return message.MessageId;
            }
            catch (Exception ex)
            {
                throw new MailFolioException(ErrorCodes.SmtpSendFailed, "Failed to send email.", ex);
            }
        }
        catch
        {
            // Let caller handle
            throw;
        }
    }

    private static MimeMessage BuildMessage(MailFolioKeyFile keyFile, MailFolioArgs args, ResolvedBody body)
    {
        var message = new MimeMessage();

        // From
        if (!string.IsNullOrWhiteSpace(keyFile.FromName))
            message.From.Add(new MailboxAddress(keyFile.FromName, keyFile.FromEmail));
        else
            message.From.Add(MailboxAddress.Parse(keyFile.FromEmail));

        // To
        message.To.Add(MailboxAddress.Parse(args.ToEmail));

        // Cc/Bcc/ReplyTo
        foreach (var cc in args.Cc) message.Cc.Add(MailboxAddress.Parse(cc));
        foreach (var bcc in args.Bcc) message.Bcc.Add(MailboxAddress.Parse(bcc));
        foreach (var rt in args.ReplyTo) message.ReplyTo.Add(MailboxAddress.Parse(rt));

        // Subject (used for sending, never stored)
        message.Subject = args.Subject ?? "";

        // Body + attachments
        var builder = new BodyBuilder();

        if (body.IsHtml || args.HtmlFlag)
            builder.HtmlBody = body.Content;
        else
            builder.TextBody = body.Content;

        foreach (var path in args.Attachments)
        {
            builder.Attachments.Add(path);
        }

        message.Body = builder.ToMessageBody();
        return message;
    }
}
```

---

# 12) `MailFolio.Console/Data/SentMailRepository.cs` → paste this

> NuGet needed:
>
> * `Microsoft.Data.Sqlite`

```csharp
using Microsoft.Data.Sqlite;
using MailFolio.Console.Reporting;

namespace MailFolio.Console.Data;

public static class SentMailRepository
{
    public static void EnsureCreated(string dbPath, string schemaSql)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

        using var conn = new SqliteConnection($"Data Source={dbPath}");
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = schemaSql;
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Upserts by ErrorCode (1 row per code). Updates LastOccurredUtc, increments OccurrenceCount, and replaces payload/context.
    /// </summary>
    public static void UpsertFailure(
        string dbPath,
        string errorCode,
        DateTimeOffset lastOccurredUtc,
        bool dryRun,
        string? fromEmail,
        string? toEmail,
        string? server,
        int? port,
        string? tlsMode,
        byte[] resultJson)
    {
        try
        {
            using var conn = new SqliteConnection($"Data Source={dbPath}");
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
INSERT INTO SentMail (
    ErrorCode, LastOccurredUtc, OccurrenceCount,
    FromEmail, ToEmail, Server, Port, TlsMode, DryRun,
    ResultJson
) VALUES (
    @ErrorCode, @LastOccurredUtc, 1,
    @FromEmail, @ToEmail, @Server, @Port, @TlsMode, @DryRun,
    @ResultJson
)
ON CONFLICT(ErrorCode) DO UPDATE SET
    LastOccurredUtc = excluded.LastOccurredUtc,
    OccurrenceCount = SentMail.OccurrenceCount + 1,
    FromEmail = excluded.FromEmail,
    ToEmail = excluded.ToEmail,
    Server = excluded.Server,
    Port = excluded.Port,
    TlsMode = excluded.TlsMode,
    DryRun = excluded.DryRun,
    ResultJson = excluded.ResultJson;
";

            cmd.Parameters.AddWithValue("@ErrorCode", errorCode);
            cmd.Parameters.AddWithValue("@LastOccurredUtc", lastOccurredUtc.UtcDateTime.ToString("O"));
            cmd.Parameters.AddWithValue("@DryRun", dryRun ? 1 : 0);

            cmd.Parameters.AddWithValue("@FromEmail", (object?)fromEmail ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ToEmail", (object?)toEmail ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Server", (object?)server ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Port", (object?)port ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TlsMode", (object?)tlsMode ?? DBNull.Value);

            cmd.Parameters.Add("@ResultJson", SqliteType.Blob).Value = resultJson;

            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            // Convert to stable error code for DB layer failures.
            throw new MailFolioException(ErrorCodes.DbWriteFailed, "Failed to write failure record to SQLite.", ex);
        }
    }
}
```

---

# 13) `MailFolio.Console/Program.cs` → paste this

This is the orchestrator that:

* reads env vars
* parses args
* loads keyfile
* decrypts credentials via KeyFolio.Core
* sends email (unless dry run)
* **writes failure only**
* outputs only `True` or `False`

```csharp
using System.Diagnostics;
using System.Text.Json;
using KeyFolio.Core;
using MailFolio.Console.Cli;
using MailFolio.Console.Config;
using MailFolio.Console.Crypto;
using MailFolio.Console.Data;
using MailFolio.Console.Mail;
using MailFolio.Console.Reporting;

namespace MailFolio.Console;

public static class Program
{
    public static int Main(string[] args)
    {
        var startedUtc = DateTimeOffset.UtcNow;
        var sw = Stopwatch.StartNew();

        // Safe context placeholders (filled as we go)
        bool dryRun = false;
        string? fromEmail = null;
        string? toEmail = null;
        string? server = null;
        int? port = null;
        string? tlsMode = null;

        string? bodySource = null;
        bool? isHtml = null;
        string[] attachments = Array.Empty<string>();

        try
        {
            // 1) Read env vars (landmine handling)
            var passphraseEnvName = "MAILFOLIO_PASSPHRASE";
            var keyFilePathEnvName = "MAILFOLIO_KEYFILE";

            var keyFilePath = RequireEnv(keyFilePathEnvName); // may throw MissingEnvVar
            _ = RequireEnv(passphraseEnvName);                // ensures it exists (secret provider will read it again)

            // 2) Parse args
            var parsed = MailFolioArgsParser.Parse(args);
            dryRun = parsed.DryRun;
            toEmail = parsed.ToEmail;
            attachments = parsed.Attachments.ToArray();

            // 3) Load keyfile
            var keyFile = KeyFileLoader.Load(keyFilePath);

            fromEmail = keyFile.FromEmail;
            server = keyFile.Server;
            port = keyFile.Port;
            tlsMode = keyFile.TlsMode;

            // 4) Resolve body mode (BodyFile => HTML else Body => text)
            var resolvedBody = MailSender.ResolveBody(parsed);
            bodySource = resolvedBody.Source;
            isHtml = resolvedBody.IsHtml;

            // 5) Decrypt SMTP credentials using KeyFolio.Core
            var keyFolio = new KeyFolio();
            var secretProvider = new EnvSecretProvider(passphraseEnvName);

            string smtpUser;
            string smtpPass;
            try
            {
                smtpUser = keyFolio.Decrypt(keyFile.UsernameEnc, secretProvider);
                smtpPass = keyFolio.Decrypt(keyFile.PasswordEnc, secretProvider);
            }
            catch (Exception ex)
            {
                throw new MailFolioException(ErrorCodes.KeyFolioDecryptFailed, "Failed to decrypt SMTP credentials.", ex);
            }

            // 6) Send (unless dry run)
            if (!dryRun)
            {
                _ = MailSender.Send(keyFile, smtpUser, smtpPass, parsed, resolvedBody);
            }

            // Success: do NOT write DB record
            Console.Out.Write("True");
            return 0;
        }
        catch (Exception ex)
        {
            var finishedUtc = DateTimeOffset.UtcNow;
            var durationMs = sw.ElapsedMilliseconds;

            // Determine stable error code
            var code = GetErrorCode(ex);

            // Build sanitized failure report (no subject/body, no username/password)
            var report = new FailureReport
            {
                ErrorCode = code,
                StartedUtc = startedUtc,
                FinishedUtc = finishedUtc,
                DurationMs = durationMs,
                DryRun = dryRun,

                FromEmail = fromEmail,
                ToEmail = toEmail,
                Server = server,
                Port = port,
                TlsMode = tlsMode,

                BodySource = bodySource,
                IsHtml = isHtml,
                Attachments = attachments,

                Error = ToErrorInfo(ex)
            };

            // Persist failure report (best effort)
            try
            {
                var dbPath = Environment.GetEnvironmentVariable("MAILFOLIO_DB")
                             ?? Path.Combine(AppContext.BaseDirectory, "Data", "MailFolio.db");

                var schemaSql = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "schema.sql"));
                SentMailRepository.EnsureCreated(dbPath, schemaSql);

                var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(report);
                SentMailRepository.UpsertFailure(
                    dbPath: dbPath,
                    errorCode: code,
                    lastOccurredUtc: finishedUtc,
                    dryRun: dryRun,
                    fromEmail: fromEmail,
                    toEmail: toEmail,
                    server: server,
                    port: port,
                    tlsMode: tlsMode,
                    resultJson: jsonBytes
                );
            }
            catch
            {
                // stay silent: user asked for no logs
            }

            Console.Out.Write("False");
            return 1;
        }
    }

    private static string RequireEnv(string name)
    {
        var v = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(v))
            throw new MailFolioException(ErrorCodes.MissingEnvVar, $"Missing required environment variable '{name}'.");
        return v;
    }

    private static string GetErrorCode(Exception ex)
    {
        // If it’s one of ours, trust it.
        if (ex is MailFolioException mf)
            return mf.ErrorCode;

        // Otherwise classify common MailKit failures if you want to extend later:
        // Keep it stable: don't explode the code list.
        return ErrorCodes.Unknown;
    }

    private static FailureReport.ErrorInfo ToErrorInfo(Exception ex) => new()
    {
        Type = ex.GetType().FullName ?? ex.GetType().Name,
        Message = ex.Message,
        StackTrace = ex.StackTrace,
        Inner = ex.InnerException is null ? null : ToErrorInfo(ex.InnerException)
    };
}
```

---

# Minimal “how to run” instructions (for the team)

### Windows PowerShell (current session)

```powershell
$env:MAILFOLIO_PASSPHRASE = "CorrectHorseBatteryStaple"
$env:MAILFOLIO_KEYFILE    = "C:\secrets\mailfolio.key.json"
# optional
$env:MAILFOLIO_DB         = "C:\MailFolio\Data\MailFolio.db"

.\MailFolio.Console.exe --to "someone@agency.mil" --subject "Test" --body "Hello"
```

### Output

* `True` on success
* `False` on failure

If they forget env vars, they’ll get:

* output: `False`
* and a SQLite failure record under `MissingEnvVar`

---

# Notes (important practical details)

### NuGet packages you’ll need

* `MailKit`
* `Microsoft.Data.Sqlite`

### KeyFile JSON reminder

Must include:

* `Server`, `Port`, `TlsMode`, `FromEmail`, `FromName`
* `UsernameEnc`, `PasswordEnc` (KeyFolio envelope strings)

---

If you want the final polish step next, I can generate:

* a `MailFolio.Console.csproj` with the two NuGet references
* a tiny `README.md` that explains env vars + examples (with “what happens if missing” in plain language)
* a `KeyFile` sample generator command (encrypt username/password into the JSON) using KeyFolio.Core
