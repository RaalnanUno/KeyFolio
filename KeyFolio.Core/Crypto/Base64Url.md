namespace KeyFolio.Core.Crypto;

internal static class Base64Url
{
    public static string Encode(ReadOnlySpan<byte> bytes)
    {
        var s = Convert.ToBase64String(bytes);
        s = s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
        return s;
    }

    public static byte[] Decode(string base64Url)
    {
        if (string.IsNullOrWhiteSpace(base64Url))
            throw new ArgumentException("Value cannot be null/empty.", nameof(base64Url));

        var s = base64Url.Replace('-', '+').Replace('_', '/');

        // Pad to multiple of 4
        switch (s.Length % 4)
        {
            case 0: break;
            case 2: s += "=="; break;
            case 3: s += "="; break;
            default: throw new FormatException("Invalid Base64Url length.");
        }

        return Convert.FromBase64String(s);
    }
}
