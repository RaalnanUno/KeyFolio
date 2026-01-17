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
