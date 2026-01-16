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
