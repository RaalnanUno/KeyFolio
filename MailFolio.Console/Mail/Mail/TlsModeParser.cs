namespace MailFolio.Console.Mail;

public static class TlsModeParser
{
    public static TlsMode Parse(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return TlsMode.StartTls;

        var v = raw.Trim();

        if (v.Equals("StartTls", StringComparison.OrdinalIgnoreCase) ||
            v.Equals("StartTLS", StringComparison.OrdinalIgnoreCase))
            return TlsMode.StartTls;

        if (v.Equals("SslOnConnect", StringComparison.OrdinalIgnoreCase) ||
            v.Equals("SSL", StringComparison.OrdinalIgnoreCase) ||
            v.Equals("Implicit", StringComparison.OrdinalIgnoreCase))
            return TlsMode.SslOnConnect;

        if (v.Equals("None", StringComparison.OrdinalIgnoreCase) ||
            v.Equals("NoTls", StringComparison.OrdinalIgnoreCase) ||
            v.Equals("Plain", StringComparison.OrdinalIgnoreCase))
            return TlsMode.None;

        // Default safe choice
        return TlsMode.StartTls;
    }
}
