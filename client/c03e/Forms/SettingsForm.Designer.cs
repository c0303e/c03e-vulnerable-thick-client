namespace c03e.Forms
{
    partial class SettingsForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.TextBox txtVault;
        private System.Windows.Forms.Button btnSaveVault;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblInfo = new System.Windows.Forms.Label();
            this.txtVault = new System.Windows.Forms.TextBox();
            this.btnSaveVault = new System.Windows.Forms.Button();
            this.SuspendLayout();

            this.lblInfo.AutoSize = true;
            this.lblInfo.Location = new System.Drawing.Point(20, 15);
            this.lblInfo.MaximumSize = new System.Drawing.Size(380, 0);
            this.lblInfo.Text = "Secure notes vault (stored at %APPDATA%\\c03e\\vault.dat):";

            this.txtVault.Location = new System.Drawing.Point(20, 45);
            this.txtVault.Multiline = true;
            this.txtVault.Size = new System.Drawing.Size(380, 120);

            this.btnSaveVault.Location = new System.Drawing.Point(20, 175);
            this.btnSaveVault.Size = new System.Drawing.Size(150, 30);
            this.btnSaveVault.Text = "Save to Vault";
            this.btnSaveVault.Click += new System.EventHandler(this.btnSaveVault_Click);

            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(420, 225);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.txtVault);
            this.Controls.Add(this.btnSaveVault);
            this.Text = "c03e - Settings / Vault";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
