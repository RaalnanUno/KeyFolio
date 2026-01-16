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
