using KeyFolio.Core.Crypto;

namespace KeyFolio.Client.Secrets;

/// <summary>
/// Gets KEYFOLIO_SECRET from environment. If missing, prompts the user.
/// Caches the secret for the current app session.
/// </summary>
public sealed class EnvOrPromptSecretProvider : ISecretProvider
{
    private readonly string _envVarName;
    private string? _cachedSecret;

    public EnvOrPromptSecretProvider(string envVarName = "KEYFOLIO_SECRET")
    {
        _envVarName = envVarName;
    }

    public string GetSecret()
    {
        if (!string.IsNullOrWhiteSpace(_cachedSecret))
            return _cachedSecret!;

        var env = Environment.GetEnvironmentVariable(_envVarName);
        if (!string.IsNullOrWhiteSpace(env))
        {
            _cachedSecret = env;
            return env;
        }

        using var dlg = new SecretPromptForm(_envVarName);
        var result = dlg.ShowDialog();

        if (result != DialogResult.OK || string.IsNullOrWhiteSpace(dlg.Secret))
            throw new InvalidOperationException($"No secret provided. Set {_envVarName} or enter it when prompted.");

        _cachedSecret = dlg.Secret!;
        return _cachedSecret!;
    }
}
