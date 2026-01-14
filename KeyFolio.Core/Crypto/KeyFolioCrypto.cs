using System.Security.Cryptography;
using System.Text;

namespace KeyFolio.Core.Crypto;

public sealed class KeyFolioCrypto
{
    private readonly KeyFolioOptions _options;

    public KeyFolioCrypto(KeyFolioOptions? options = null)
    {
        _options = options ?? new KeyFolioOptions();

        if (_options.Pbkdf2Iterations < 10_000)
            throw new ArgumentOutOfRangeException(nameof(_options.Pbkdf2Iterations), "PBKDF2 iterations too low.");

        if (_options.KeySizeBytes is not (16 or 24 or 32))
            throw new ArgumentOutOfRangeException(nameof(_options.KeySizeBytes), "AES key must be 16/24/32 bytes.");

        if (_options.NonceSizeBytes != 12)
        {
            // AES-GCM supports other nonce sizes, but 12 is the best default.
            // Keep it strict to avoid surprises.
            throw new ArgumentOutOfRangeException(nameof(_options.NonceSizeBytes), "AES-GCM nonce should be 12 bytes.");
        }
    }

    public string Encrypt(string plaintext, ISecretProvider secretProvider)
    {
        if (plaintext is null) throw new ArgumentNullException(nameof(plaintext));
        if (secretProvider is null) throw new ArgumentNullException(nameof(secretProvider));

        var secret = secretProvider.GetSecret();
        if (string.IsNullOrWhiteSpace(secret))
            throw new InvalidOperationException("Secret provider returned an empty secret.");

        var plainBytes = Encoding.UTF8.GetBytes(plaintext);

        var salt = RandomNumberGenerator.GetBytes(_options.SaltSizeBytes);
        var nonce = RandomNumberGenerator.GetBytes(_options.NonceSizeBytes);

        var key = DeriveKey(secret, salt, _options);

        // AES-GCM outputs: ciphertext + tag
        var ciphertext = new byte[plainBytes.Length];
        var tag = new byte[16]; // 128-bit tag

        using (var aes = new AesGcm(key, tagSizeInBytes: tag.Length))
        {
            var aad = Encoding.UTF8.GetBytes(_options.Aad);
            aes.Encrypt(nonce, plainBytes, ciphertext, tag, aad);
        }

        // Combine ciphertext + tag for compactness
        var ctWithTag = new byte[ciphertext.Length + tag.Length];
        Buffer.BlockCopy(ciphertext, 0, ctWithTag, 0, ciphertext.Length);
        Buffer.BlockCopy(tag, 0, ctWithTag, ciphertext.Length, tag.Length);

        return new KeyFolioEnvelope(KeyFolioEnvelope.ExpectedVersion, salt, nonce, ctWithTag).ToString();
    }

    public string Decrypt(string envelopeText, ISecretProvider secretProvider)
    {
        if (envelopeText is null) throw new ArgumentNullException(nameof(envelopeText));
        if (secretProvider is null) throw new ArgumentNullException(nameof(secretProvider));

        var secret = secretProvider.GetSecret();
        if (string.IsNullOrWhiteSpace(secret))
            throw new InvalidOperationException("Secret provider returned an empty secret.");

        var env = KeyFolioEnvelope.Parse(envelopeText);

        // Validate sizes
        if (env.Salt.Length != _options.SaltSizeBytes)
            throw new CryptographicException("Invalid salt size.");

        if (env.Nonce.Length != _options.NonceSizeBytes)
            throw new CryptographicException("Invalid nonce size.");

        if (env.CiphertextWithTag.Length < 16)
            throw new CryptographicException("Invalid ciphertext/tag payload.");

        var key = DeriveKey(secret, env.Salt, _options);

        var ctLen = env.CiphertextWithTag.Length - 16;
        var ciphertext = new byte[ctLen];
        var tag = new byte[16];

        Buffer.BlockCopy(env.CiphertextWithTag, 0, ciphertext, 0, ctLen);
        Buffer.BlockCopy(env.CiphertextWithTag, ctLen, tag, 0, 16);

        var plaintext = new byte[ctLen];

        using (var aes = new AesGcm(key, tagSizeInBytes: tag.Length))
        {
            var aad = Encoding.UTF8.GetBytes(_options.Aad);
            aes.Decrypt(env.Nonce, ciphertext, tag, plaintext, aad);
        }

        return Encoding.UTF8.GetString(plaintext);
    }

    private static byte[] DeriveKey(string secret, byte[] salt, KeyFolioOptions options)
    {
        // PBKDF2-SHA256 to derive 32-byte AES key
        using var kdf = new Rfc2898DeriveBytes(
            password: secret,
            salt: salt,
            iterations: options.Pbkdf2Iterations,
            hashAlgorithm: HashAlgorithmName.SHA256);

        return kdf.GetBytes(options.KeySizeBytes);
    }
}
