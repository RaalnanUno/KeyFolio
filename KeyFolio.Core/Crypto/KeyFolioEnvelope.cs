namespace KeyFolio.Core.Crypto;

/// <summary>
/// Self-describing envelope for ciphertext material.
/// Format:
///   keyfolio:v1:&lt;salt&gt;.&lt;nonce&gt;.&lt;ct&gt;
/// Where salt/nonce/ct are Base64Url. ct contains ciphertext+tag.
/// </summary>
public sealed record KeyFolioEnvelope(
    string Version,
    byte[] Salt,
    byte[] Nonce,
    byte[] CiphertextWithTag
)
{
    public const string ExpectedVersion = "keyfolio:v1";

    public override string ToString()
        => $"{ExpectedVersion}:{Base64Url.Encode(Salt)}.{Base64Url.Encode(Nonce)}.{Base64Url.Encode(CiphertextWithTag)}";

    public static KeyFolioEnvelope Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Envelope cannot be null/empty.", nameof(input));

        // prefix split at first ':'
        var idx = input.IndexOf(':');
        if (idx < 0) throw new FormatException("Invalid envelope: missing ':'.");

        var version = input[..idx].Trim();
        var rest = input[(idx + 1)..];

        // But our version itself contains ':' (keyfolio:v1). So handle properly:
        // We expect "keyfolio:v1:<parts>"
        const string prefix = ExpectedVersion + ":";
        if (!input.StartsWith(prefix, StringComparison.Ordinal))
            throw new FormatException($"Invalid envelope: expected prefix '{prefix}'.");

        rest = input[prefix.Length..];

        var parts = rest.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 3)
            throw new FormatException("Invalid envelope: expected 3 dot-separated parts (salt.nonce.ct).");

        var salt = Base64Url.Decode(parts[0]);
        var nonce = Base64Url.Decode(parts[1]);
        var ct = Base64Url.Decode(parts[2]);

        return new KeyFolioEnvelope(ExpectedVersion, salt, nonce, ct);
    }
}
