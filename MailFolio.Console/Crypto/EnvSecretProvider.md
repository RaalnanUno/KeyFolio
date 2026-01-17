using KeyFolio.Core.Crypto;

namespace MailFolio.Console.Crypto;

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
            throw new InvalidOperationException($"Missing required environment variable '{_envVar}'.");
        return v;
    }
}
