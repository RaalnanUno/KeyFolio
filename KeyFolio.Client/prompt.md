## Form1.cs

```cs
using KeyFolio.Client.Secrets;
using KeyFolio.Core;

namespace KeyFolio.Client;

public partial class Form1 : Form
{
    private readonly KeyFolio.Core.KeyFolio _keyfolio = new();
    private readonly EnvOrPromptSecretProvider _secretProvider = new("KEYFOLIO_SECRET");

    public Form1()
    {
        InitializeComponent();

        btnEncrypt.Click += (_, __) => Encrypt();
        btnDecrypt.Click += (_, __) => Decrypt();
        btnCopyOutput.Click += (_, __) => CopyOutput();

        txtInput.Text = "Hello from KeyFolio (JSON/plain text both fine).";
        lblStatus.Text = "Ready.";
    }

    private void Encrypt()
    {
        try
        {
            var input = txtInput.Text ?? string.Empty;
            var encrypted = _keyfolio.Encrypt(input, _secretProvider);
            txtOutput.Text = encrypted;
            lblStatus.Text = "Encrypted successfully.";
        }
        catch (Exception ex)
        {
            lblStatus.Text = $"Encrypt failed: {ex.Message}";
            MessageBox.Show(this, ex.ToString(), "Encrypt Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Decrypt()
    {
        try
        {
            var input = txtInput.Text ?? string.Empty;
            var decrypted = _keyfolio.Decrypt(input, _secretProvider);
            txtOutput.Text = decrypted;
            lblStatus.Text = "Decrypted successfully.";
        }
        catch (Exception ex)
        {
            lblStatus.Text = $"Decrypt failed: {ex.Message}";
            MessageBox.Show(this, ex.ToString(), "Decrypt Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void CopyOutput()
    {
        try
        {
            Clipboard.SetText(txtOutput.Text ?? string.Empty);
            lblStatus.Text = "Output copied to clipboard.";
        }
        catch (Exception ex)
        {
            lblStatus.Text = $"Copy failed: {ex.Message}";
        }
    }
}

```

## Secrets\EnvOrPromptSecretProvider.cs

```cs
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

```