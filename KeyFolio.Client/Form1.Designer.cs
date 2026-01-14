namespace KeyFolio.Client
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        private TextBox txtInput;
        private TextBox txtOutput;
        private Button btnEncrypt;
        private Button btnDecrypt;
        private Button btnCopyOutput;
        private Label lblInput;
        private Label lblOutput;
        private Label lblStatus;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            txtInput = new TextBox();
            txtOutput = new TextBox();
            btnEncrypt = new Button();
            btnDecrypt = new Button();
            btnCopyOutput = new Button();
            lblInput = new Label();
            lblOutput = new Label();
            lblStatus = new Label();
            SuspendLayout();
            // 
            // lblInput
            // 
            lblInput.AutoSize = true;
            lblInput.Location = new Point(12, 12);
            lblInput.Name = "lblInput";
            lblInput.Size = new Size(185, 15);
            lblInput.TabIndex = 0;
            lblInput.Text = "Input (plain text OR envelope)";
            // 
            // txtInput
            // 
            txtInput.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtInput.Font = new Font("Consolas", 10F);
            txtInput.Location = new Point(12, 34);
            txtInput.Multiline = true;
            txtInput.Name = "txtInput";
            txtInput.ScrollBars = ScrollBars.Vertical;
            txtInput.Size = new Size(960, 220);
            txtInput.TabIndex = 1;
            // 
            // btnEncrypt
            // 
            btnEncrypt.Location = new Point(12, 270);
            btnEncrypt.Name = "btnEncrypt";
            btnEncrypt.Size = new Size(120, 36);
            btnEncrypt.TabIndex = 2;
            btnEncrypt.Text = "Encrypt →";
            btnEncrypt.UseVisualStyleBackColor = true;
            // 
            // btnDecrypt
            // 
            btnDecrypt.Location = new Point(142, 270);
            btnDecrypt.Name = "btnDecrypt";
            btnDecrypt.Size = new Size(120, 36);
            btnDecrypt.TabIndex = 3;
            btnDecrypt.Text = "Decrypt →";
            btnDecrypt.UseVisualStyleBackColor = true;
            // 
            // btnCopyOutput
            // 
            btnCopyOutput.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCopyOutput.Location = new Point(852, 270);
            btnCopyOutput.Name = "btnCopyOutput";
            btnCopyOutput.Size = new Size(120, 36);
            btnCopyOutput.TabIndex = 4;
            btnCopyOutput.Text = "Copy Output";
            btnCopyOutput.UseVisualStyleBackColor = true;
            // 
            // lblOutput
            // 
            lblOutput.AutoSize = true;
            lblOutput.Location = new Point(12, 320);
            lblOutput.Name = "lblOutput";
            lblOutput.Size = new Size(45, 15);
            lblOutput.TabIndex = 5;
            lblOutput.Text = "Output";
            // 
            // txtOutput
            // 
            txtOutput.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtOutput.Font = new Font("Consolas", 10F);
            txtOutput.Location = new Point(12, 342);
            txtOutput.Multiline = true;
            txtOutput.Name = "txtOutput";
            txtOutput.ScrollBars = ScrollBars.Vertical;
            txtOutput.Size = new Size(960, 330);
            txtOutput.TabIndex = 6;
            // 
            // lblStatus
            // 
            lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblStatus.Location = new Point(12, 680);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(960, 23);
            lblStatus.TabIndex = 7;
            lblStatus.Text = "Ready.";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(984, 712);
            Controls.Add(lblStatus);
            Controls.Add(txtOutput);
            Controls.Add(lblOutput);
            Controls.Add(btnCopyOutput);
            Controls.Add(btnDecrypt);
            Controls.Add(btnEncrypt);
            Controls.Add(txtInput);
            Controls.Add(lblInput);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "KeyFolio Client";
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
