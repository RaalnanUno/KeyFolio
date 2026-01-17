using KeyFolio.Core.Crypto;

namespace KeyFolio.Core;

/// <summary>
/// Public façade for KeyFolio encryption and decryption.
/// Consumers should start here.
/// </summary>
public sealed class KeyFolio
{
    private readonly KeyFolioCrypto _crypto;

    public KeyFolio(KeyFolioOptions? options = null)
    {
        _crypto = new KeyFolioCrypto(options);
    }

    /// <summary>
    /// Encrypts a UTF-8 string and returns a portable, versioned envelope string.
    /// </summary>
    public string Encrypt(string plaintext, ISecretProvider secretProvider)
        => _crypto.Encrypt(plaintext, secretProvider);

    /// <summary>
    /// Decrypts a KeyFolio envelope string back into the original UTF-8 text.
    /// </summary>
    public string Decrypt(string envelope, ISecretProvider secretProvider)
        => _crypto.Decrypt(envelope, secretProvider);
}
