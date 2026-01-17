using KeyFolio.Core.Crypto;

namespace MailFolio.Console.Crypto;
using MailFolio.Console.Reporting;

public sealed class EnvSecretProvider : ISecretProvider
{
    private readonly string _envVar;

    public EnvSecretProvider(string envVar)
    {
        _envVar = envVar ?? throw new ArgumentNullException(nameof(envVar));
    }

    public string GetSecret()
    {
        var v = Environment.GetEnvironmentVariable(_envVar);
        if (string.IsNullOrWhiteSpace(v))
            throw new MailFolioException(ErrorCodes.MissingEnvVar, $"Missing required environment variable '{_envVar}'.");

        return v;
    }
}
