namespace KeyFolio.Core.Crypto;

public sealed record KeyFolioOptions
{
    /// <summary>Envelope prefix + AAD string (versioned).</summary>
    public string Aad { get; init; } = "keyfolio:v1";

    /// <summary>PBKDF2 iteration count. (210k is a decent modern baseline.)</summary>
    public int Pbkdf2Iterations { get; init; } = 210_000;

    /// <summary>Salt size in bytes (per message).</summary>
    public int SaltSizeBytes { get; init; } = 16;

    /// <summary>Nonce size for AES-GCM in bytes (12 recommended).</summary>
    public int NonceSizeBytes { get; init; } = 12;

    /// <summary>Key size in bytes (32 = AES-256).</summary>
    public int KeySizeBytes { get; init; } = 32;
}
