namespace c03e.Forms
{
    partial class ProfileForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblU, lblF, lblE, lblR, lblT;
        private System.Windows.Forms.TextBox txtUsername, txtFullName, txtEmail, txtRole, txtToken;
        private System.Windows.Forms.Button btnShowStoredSecrets;
        private System.Windows.Forms.TextBox txtStoredSecrets;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblU = new System.Windows.Forms.Label();
            this.lblF = new System.Windows.Forms.Label();
            this.lblE = new System.Windows.Forms.Label();
            this.lblR = new System.Windows.Forms.Label();
            this.lblT = new System.Windows.Forms.Label();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.txtFullName = new System.Windows.Forms.TextBox();
            this.txtEmail = new System.Windows.Forms.TextBox();
            this.txtRole = new System.Windows.Forms.TextBox();
            this.txtToken = new System.Windows.Forms.TextBox();
            this.btnShowStoredSecrets = new System.Windows.Forms.Button();
            this.txtStoredSecrets = new System.Windows.Forms.TextBox();
            this.SuspendLayout();

            int y = 20;
            void AddRow(System.Windows.Forms.Label lbl, string text, System.Windows.Forms.TextBox tb, bool readOnly)
            {
                lbl.AutoSize = true;
                lbl.Location = new System.Drawing.Point(20, y);
                lbl.Text = text;
                tb.Location = new System.Drawing.Point(140, y - 3);
                tb.Size = new System.Drawing.Size(300, 23);
                tb.ReadOnly = readOnly;
                y += 32;
            }

            AddRow(this.lblU, "Username:", this.txtUsername, true);
            AddRow(this.lblF, "Full name:", this.txtFullName, true);
            AddRow(this.lblE, "Email:", this.txtEmail, true);
            AddRow(this.lblR, "Role:", this.txtRole, true);
            AddRow(this.lblT, "Session token:", this.txtToken, true);

            this.btnShowStoredSecrets.Location = new System.Drawing.Point(20, y + 5);
            this.btnShowStoredSecrets.Size = new System.Drawing.Size(220, 28);
            this.btnShowStoredSecrets.Text = "Show what's cached on disk";
            this.btnShowStoredSecrets.Click += new System.EventHandler(this.btnShowStoredSecrets_Click);

            this.txtStoredSecrets.Location = new System.Drawing.Point(20, y + 40);
            this.txtStoredSecrets.Multiline = true;
            this.txtStoredSecrets.ReadOnly = true;
            this.txtStoredSecrets.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtStoredSecrets.Size = new System.Drawing.Size(420, 140);

            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(460, y + 200);
            this.Controls.Add(this.lblU); this.Controls.Add(this.txtUsername);
            this.Controls.Add(this.lblF); this.Controls.Add(this.txtFullName);
            this.Controls.Add(this.lblE); this.Controls.Add(this.txtEmail);
            this.Controls.Add(this.lblR); this.Controls.Add(this.txtRole);
            this.Controls.Add(this.lblT); this.Controls.Add(this.txtToken);
            this.Controls.Add(this.btnShowStoredSecrets);
            this.Controls.Add(this.txtStoredSecrets);
            this.Text = "c03e - My Profile";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.ProfileForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
