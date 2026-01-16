namespace KeyFolio.Core.Crypto;

/// <summary>Provides the shared passphrase (NOT the derived AES key).</summary>
public interface ISecretProvider
{
    string GetSecret();
}

/// <summary>
/// Simple provider that wraps a provided string.
/// Useful for tests and callers that already have the secret.
/// </summary>
public sealed class DirectSecretProvider : ISecretProvider
{
    private readonly string _secret;
    public DirectSecretProvider(string secret)
    {
        _secret = secret ?? throw new ArgumentNullException(nameof(secret));
    }
    public string GetSecret() => _secret;
}
