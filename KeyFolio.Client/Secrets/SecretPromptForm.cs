namespace KeyFolio.Client.Secrets;

public sealed class SecretPromptForm : Form
{
    private readonly TextBox _txtSecret;

    public string? Secret => _txtSecret.Text;

    public SecretPromptForm(string envVarName)
    {
        Text = "KeyFolio Secret Required";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        Width = 520;
        Height = 200;

        var lbl = new Label
        {
            AutoSize = false,
            Left = 12,
            Top = 12,
            Width = 480,
            Height = 48,
            Text = $"Environment variable '{envVarName}' was not found.\r\nEnter the shared passphrase to encrypt/decrypt:",
        };

        _txtSecret = new TextBox
        {
            Left = 12,
            Top = 68,
            Width = 480,
            UseSystemPasswordChar = true
        };

        var chkShow = new CheckBox
        {
            Left = 12,
            Top = 98,
            Width = 180,
            Text = "Show passphrase"
        };
        chkShow.CheckedChanged += (_, __) => _txtSecret.UseSystemPasswordChar = !chkShow.Checked;

        var btnOk = new Button
        {
            Text = "OK",
            Left = 312,
            Top = 126,
            Width = 85,
            DialogResult = DialogResult.OK
        };

        var btnCancel = new Button
        {
            Text = "Cancel",
            Left = 407,
            Top = 126,
            Width = 85,
            DialogResult = DialogResult.Cancel
        };

        AcceptButton = btnOk;
        CancelButton = btnCancel;

        Controls.Add(lbl);
        Controls.Add(_txtSecret);
        Controls.Add(chkShow);
        Controls.Add(btnOk);
        Controls.Add(btnCancel);
    }
}
