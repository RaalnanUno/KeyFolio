using System.Text.Json;
using MailFolio.Console;
using MailFolio.Console.Cli;
using MailFolio.Console.Config;
using MailFolio.Console.Crypto;
using MailFolio.Console.Data;
using MailFolio.Console.Mail;
using MailFolio.Console.Reporting;

static string RequireEnv(string name, string errorCode)
{
    var v = Environment.GetEnvironmentVariable(name);
    if (string.IsNullOrWhiteSpace(v))
        throw new MailFolioException(errorCode, $"Missing required environment variable '{name}'.");
    return v;
}

static string ResolveDbPath()
{
    var v = Environment.GetEnvironmentVariable("MAILFOLIO_DB");
    if (!string.IsNullOrWhiteSpace(v))
        return v;

    // Default under exe folder for stability
    var baseDir = AppContext.BaseDirectory;
    return Path.Combine(baseDir, "Data", "MailFolio.db");
}

static string LoadSchemaSql()
{
    // Copy-to-output (see csproj below)
    var schemaPath = Path.Combine(AppContext.BaseDirectory, "Sql", "schema.sql");
    if (!File.Exists(schemaPath))
        throw new MailFolioException(ErrorCodes.InvalidKeyFile, $"Missing schema file at '{schemaPath}'. Ensure Sql\\schema.sql is copied to output.");
    return File.ReadAllText(schemaPath);
}

var startedUtc = DateTimeOffset.UtcNow;

// DEBUG: Dump KeyFolioOptions shape
if (args.Any(a => a.Equals("--dumpKeyFolio", StringComparison.OrdinalIgnoreCase)))
{
    try
    {
        var optionsType = Type.GetType("KeyFolio.Core.Crypto.KeyFolioOptions, KeyFolio.Core", throwOnError: true)!;

        Out.Info($"KeyFolioOptions: {optionsType.FullName}");
        Out.Info("Constructors:");
        foreach (var c in optionsType.GetConstructors())
        {
            var ps = c.GetParameters();
            Out.Info("  " + optionsType.Name + "(" + string.Join(", ", ps.Select(p => $"{p.ParameterType.Name} {p.Name}")) + ")");
        }

        Out.Info("Properties:");
        foreach (var p in optionsType.GetProperties())
        {
            Out.Info($"  {p.PropertyType.Name} {p.Name}  CanWrite={p.CanWrite}");
        }

        return 0;
    }
    catch (Exception ex)
    {
        Out.Error($"Dump failed: {ex.GetType().Name}: {ex.Message}");
        return 1;
    }
}

if (args.Any(a => a.Equals("--dumpKeyFolioCrypto", StringComparison.OrdinalIgnoreCase)))
{
    try
    {
        var cryptoType = Type.GetType("KeyFolio.Core.Crypto.KeyFolioCrypto, KeyFolio.Core", throwOnError: true)!;

        Out.Info($"KeyFolioCrypto: {cryptoType.FullName}");

        Out.Info("Constructors:");
        foreach (var c in cryptoType.GetConstructors())
        {
            var ps = c.GetParameters();
            Out.Info("  " + cryptoType.Name + "(" + string.Join(", ", ps.Select(p => $"{p.ParameterType.FullName} {p.Name}")) + ")");
        }

        Out.Info("Methods containing 'Decrypt' or 'Unprotect':");
        foreach (var m in cryptoType.GetMethods().Where(m =>
                     m.Name.Contains("Decrypt", StringComparison.OrdinalIgnoreCase) ||
                     m.Name.Contains("Unprotect", StringComparison.OrdinalIgnoreCase)))
        {
            var ps = m.GetParameters();
            Out.Info($"  {m.ReturnType.Name} {m.Name}({string.Join(", ", ps.Select(p => $"{p.ParameterType.Name} {p.Name}"))})");
        }

        return 0;
    }
    catch (Exception ex)
    {
        Out.Error($"Dump failed: {ex.GetType().Name}: {ex.Message}");
        return 1;
    }
}


try
{
    var parsed = MailFolioArgsParser.Parse(args);

    var keyFilePath = RequireEnv("MAILFOLIO_KEYFILE", ErrorCodes.MissingEnvVar);
    _ = RequireEnv("MAILFOLIO_PASSPHRASE", ErrorCodes.MissingEnvVar); // validated by EnvSecretProvider later

    var dbPath = ResolveDbPath();
    var schemaSql = LoadSchemaSql();
    SentMailRepository.EnsureCreated(dbPath, schemaSql);

    var keyFile = KeyFileLoader.Load(keyFilePath);

    // Decrypt creds
    var smtpUsername = KeyFolioBridge.DecryptWithPassphraseEnv("MAILFOLIO_PASSPHRASE", keyFile.UsernameEnc);
    var smtpPassword = KeyFolioBridge.DecryptWithPassphraseEnv("MAILFOLIO_PASSPHRASE", keyFile.PasswordEnc);

    var body = MailSender.ResolveBody(parsed);

    if (parsed.DryRun)
    {
        Out.Info("[DRY RUN] Parsed OK. Skipping send.");
        return 0;
    }

    var messageId = MailSender.Send(keyFile, smtpUsername, smtpPassword, parsed, body);

    Out.Info($"Sent. Message-Id: {messageId ?? "(none)"}");
    return 0;
}
catch (MailFolioException ex)
{
    var finishedUtc = DateTimeOffset.UtcNow;

    // Build safe failure report (no subject/body/creds)
    FailureReport report = new()
    {
        ErrorCode = ex.ErrorCode,
        StartedUtc = startedUtc,
        FinishedUtc = finishedUtc,
        DurationMs = (long)(finishedUtc - startedUtc).TotalMilliseconds,

        // best-effort: parse args safely if possible for context
        DryRun = args.Contains("--dryRun", StringComparer.OrdinalIgnoreCase),

        Error = new FailureReport.ErrorInfo
        {
            Type = ex.GetType().FullName ?? "Exception",
            Message = ex.Message,
            StackTrace = ex.StackTrace,
            Inner = ex.InnerException is null ? null : new FailureReport.ErrorInfo
            {
                Type = ex.InnerException.GetType().FullName ?? "Exception",
                Message = ex.InnerException.Message,
                StackTrace = ex.InnerException.StackTrace
            }
        }
    };

    // Try to enrich context from argv without re-parsing deeply
    string? toEmail = TryGetArgValue(args, "--to");
    report = report with { ToEmail = toEmail };

    // Keyfile context if possible
    var keyFilePath = Environment.GetEnvironmentVariable("MAILFOLIO_KEYFILE");
    MailFolioKeyFile? keyFile = null;
    if (!string.IsNullOrWhiteSpace(keyFilePath) && File.Exists(keyFilePath))
    {
        try { keyFile = KeyFileLoader.Load(keyFilePath); } catch { /* ignore */ }
    }

    report = report with
    {
        FromEmail = keyFile?.FromEmail,
        Server = keyFile?.Server,
        Port = keyFile?.Port,
        TlsMode = keyFile?.TlsMode
    };

    var dbPath = ResolveDbPath();

    try
    {
        var schemaSql = LoadSchemaSql();
        SentMailRepository.EnsureCreated(dbPath, schemaSql);

        var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(report, new JsonSerializerOptions { WriteIndented = true });

        SentMailRepository.UpsertFailure(
            dbPath: dbPath,
            errorCode: report.ErrorCode,
            lastOccurredUtc: finishedUtc,
            dryRun: report.DryRun,
            fromEmail: report.FromEmail,
            toEmail: report.ToEmail,
            server: report.Server,
            port: report.Port,
            tlsMode: report.TlsMode,
            resultJson: jsonBytes
        );
    }
    catch (Exception writeEx)
    {
        Out.Error($"Failed to write failure report to DB: {writeEx.Message}");
    }

    if (ex.ErrorCode == ErrorCodes.KeyFolioDecryptFailed && ex.InnerException is not null)
        Out.Error($"Inner: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");

    return 2;
}
catch (Exception ex)
{
    Out.Error($"MailFolio failed: [{ErrorCodes.Unknown}] {ex.Message}");
    return 99;
}

static string? TryGetArgValue(string[] argv, string key)
{
    for (int i = 0; i < argv.Length - 1; i++)
    {
        if (argv[i].Equals(key, StringComparison.OrdinalIgnoreCase))
            return argv[i + 1];
    }
    return null;
}
