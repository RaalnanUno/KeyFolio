using MailFolio.Console.Reporting;

namespace MailFolio.Console.Crypto;

public static class KeyFolioBridge
{
    /// <summary>
    /// Creates KeyFolio.Core.Crypto.KeyFolioCrypto using EnvSecretProvider and tries common decrypt method names.
    /// This compiles even if the KeyFolio API changes slightly; failures surface as MailFolioException.
    /// </summary>
    public static string DecryptWithPassphraseEnv(string envVarName, string encrypted)
    {
        try
        {
            var asmQualified = "KeyFolio.Core.Crypto.KeyFolioCrypto, KeyFolio.Core";
            var t = Type.GetType(asmQualified, throwOnError: true)!;

            var provider = new EnvSecretProvider(envVarName);

            // Try ctor(ISecretProvider) first
            object crypto;
            var ctor = t.GetConstructor(new[] { provider.GetType().GetInterfaces().FirstOrDefault() ?? provider.GetType() })
                      ?? t.GetConstructors().FirstOrDefault(c => c.GetParameters().Length == 1);

            if (ctor is null)
                throw new InvalidOperationException("KeyFolioCrypto constructor not found.");

            crypto = ctor.Invoke(new object[] { provider });

            // Try common method names
            var methodsToTry = new[]
            {
                "Decrypt",
                "DecryptString",
                "DecryptEnvelopeString",
                "Unprotect",
                "UnprotectString"
            };

            foreach (var name in methodsToTry)
            {
                var m = t.GetMethod(name, new[] { typeof(string) });
                if (m is null) continue;

                var result = m.Invoke(crypto, new object[] { encrypted }) as string;
                if (!string.IsNullOrWhiteSpace(result))
                    return result!;
            }

            throw new InvalidOperationException("No compatible decrypt method found on KeyFolioCrypto.");
        }
        catch (MailFolioException) { throw; }
        catch (Exception ex)
        {
            throw new MailFolioException(ErrorCodes.KeyFolioDecryptFailed, "Failed to decrypt credentials using KeyFolio.", ex);
        }
    }
}
