using MailFolio.Console.Reporting;

namespace MailFolio.Console.Crypto;

public static class KeyFolioBridge
{
    public static string DecryptWithPassphraseEnv(string envVarName, string encrypted)
    {
        try
        {
            // Build KeyFolioOptions (defaults are fine)
            var optionsType = Type.GetType("KeyFolio.Core.Crypto.KeyFolioOptions, KeyFolio.Core", throwOnError: true)!;
            var cryptoType = Type.GetType("KeyFolio.Core.Crypto.KeyFolioCrypto, KeyFolio.Core", throwOnError: true)!;
            var secretProviderType = Type.GetType("KeyFolio.Core.Crypto.ISecretProvider, KeyFolio.Core", throwOnError: true)!;

            var options = Activator.CreateInstance(optionsType)
                          ?? throw new InvalidOperationException("Failed to create KeyFolioOptions.");

            // new KeyFolioCrypto(options)
            var ctor = cryptoType.GetConstructor(new[] { optionsType })
                      ?? throw new InvalidOperationException("KeyFolioCrypto(KeyFolioOptions) constructor not found.");

            var crypto = ctor.Invoke(new[] { options });

            // EnvSecretProvider implements KeyFolio.Core.Crypto.ISecretProvider
            var provider = new EnvSecretProvider(envVarName);
            if (!secretProviderType.IsInstanceOfType(provider))
                throw new InvalidOperationException("EnvSecretProvider does not implement KeyFolio.Core.Crypto.ISecretProvider.");

            // Call: string Decrypt(string envelopeText, ISecretProvider secretProvider)
            var decrypt = cryptoType.GetMethod("Decrypt", new[] { typeof(string), secretProviderType })
                         ?? throw new InvalidOperationException("Decrypt(string, ISecretProvider) method not found.");

            var result = decrypt.Invoke(crypto, new object[] { encrypted, provider }) as string;

            if (string.IsNullOrWhiteSpace(result))
                throw new InvalidOperationException("KeyFolio decrypt returned empty result.");

            return result!;
        }
        catch (MailFolioException) { throw; }
        catch (Exception ex)
        {
            throw new MailFolioException(ErrorCodes.KeyFolioDecryptFailed, "Failed to decrypt credentials using KeyFolio.", ex);
        }
    }
}
