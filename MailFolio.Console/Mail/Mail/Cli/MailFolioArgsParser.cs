using MailFolio.Console.Reporting;
using System.Net.Mail;

namespace MailFolio.Console.Cli;

public static class MailFolioArgsParser
{
    public static MailFolioArgs Parse(string[] argv)
    {
        var a = new MailFolioArgs();

        string? pendingKey = null;

        void AddList(List<string> list, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                list.Add(value);
        }

        for (var i = 0; i < argv.Length; i++)
        {
            var token = argv[i];

            if (pendingKey is not null)
            {
                Apply(pendingKey, token);
                pendingKey = null;
                continue;
            }

            if (token is "--help" or "-h" or "/?")
                throw new MailFolioException(ErrorCodes.InvalidArgs, Usage());

            if (!token.StartsWith("-", StringComparison.Ordinal))
            {
                // Ignore stray tokens (or treat as invalid)
                throw new MailFolioException(ErrorCodes.InvalidArgs, $"Unexpected token '{token}'.\n\n{Usage()}");
            }

            if (IsFlag(token))
            {
                Apply(token, "true");
                continue;
            }

            // Key/value option: allow --key value
            pendingKey = token;
        }

        if (pendingKey is not null)
            throw new MailFolioException(ErrorCodes.InvalidArgs, $"Missing value for '{pendingKey}'.\n\n{Usage()}");

        NormalizeAndValidate(a);
        return a;

        void Apply(string key, string value)
        {
            switch (key)
            {
                case "--to":
                    a.ToEmail = value;
                    break;

                case "--subject":
                    a.Subject = value ?? "";
                    break;

                case "--body":
                    a.Body = value;
                    break;

                case "--bodyFile":
                    a.BodyFile = value;
                    break;

                case "--attach":
                case "--attachment":
                    AddList(a.Attachments, value);
                    break;

                case "--cc":
                    AddList(a.Cc, value);
                    break;

                case "--bcc":
                    AddList(a.Bcc, value);
                    break;

                case "--replyTo":
                    AddList(a.ReplyTo, value);
                    break;

                case "--dryRun":
                    a.DryRun = ParseBool(value);
                    break;

                case "--html":
                    a.HtmlFlag = ParseBool(value);
                    break;

                case "--verbose":
                    a.Verbose = ParseBool(value);
                    break;

                default:
                    throw new MailFolioException(ErrorCodes.InvalidArgs, $"Unknown option '{key}'.\n\n{Usage()}");
            }
        }
    }

    private static bool IsFlag(string token)
        => token is "--dryRun" or "--html" or "--verbose";

    private static bool ParseBool(string? v)
    {
        if (string.IsNullOrWhiteSpace(v)) return true;
        if (v.Equals("true", StringComparison.OrdinalIgnoreCase)) return true;
        if (v.Equals("false", StringComparison.OrdinalIgnoreCase)) return false;
        if (v.Equals("1", StringComparison.OrdinalIgnoreCase)) return true;
        if (v.Equals("0", StringComparison.OrdinalIgnoreCase)) return false;
        return true;
    }

    private static void NormalizeAndValidate(MailFolioArgs a)
    {
        if (string.IsNullOrWhiteSpace(a.ToEmail))
            throw new MailFolioException(ErrorCodes.InvalidArgs, "Missing required '--to'.\n\n" + Usage());

        ValidateEmail(a.ToEmail, ErrorCodes.InvalidEmailAddress);

        foreach (var x in a.Cc) ValidateEmail(x, ErrorCodes.InvalidEmailAddress);
        foreach (var x in a.Bcc) ValidateEmail(x, ErrorCodes.InvalidEmailAddress);
        foreach (var x in a.ReplyTo) ValidateEmail(x, ErrorCodes.InvalidEmailAddress);

        if (string.IsNullOrWhiteSpace(a.Body) && string.IsNullOrWhiteSpace(a.BodyFile))
            throw new MailFolioException(ErrorCodes.MissingBody, "Either '--body' or '--bodyFile' must be provided.\n\n" + Usage());

        if (!string.IsNullOrWhiteSpace(a.BodyFile) && !File.Exists(a.BodyFile))
            throw new MailFolioException(ErrorCodes.BodyFileNotFound, $"Body file not found: '{a.BodyFile}'.");

        for (int i = 0; i < a.Attachments.Count; i++)
        {
            var p = a.Attachments[i];
            if (!File.Exists(p))
                throw new MailFolioException(ErrorCodes.AttachmentNotFound, $"Attachment not found: '{p}'.");
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
            throw new MailFolioException(errorCode, $"Invalid email address: '{email}'.");
        }
    }

    public static string Usage() =>
@"MailFolio.Console

Required:
  --to <email>                 Recipient email address
  --body <text> OR --bodyFile <path>    Body text or HTML file

Optional:
  --subject <text>
  --attach <path>              Repeatable
  --cc <email>                 Repeatable
  --bcc <email>                Repeatable
  --replyTo <email>            Repeatable
  --dryRun                     Do everything except send
  --html                       Treat --body as HTML
  --verbose

Examples:
  .\MailFolio.Console.exe --to someone@agency.mil --subject ""Test"" --body ""Hello""
  .\MailFolio.Console.exe --to someone@agency.mil --subject ""Test"" --bodyFile .\email.html --attach .\a.pdf --dryRun
";
}
