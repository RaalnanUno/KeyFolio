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
