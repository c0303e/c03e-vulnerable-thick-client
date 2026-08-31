namespace c03e.Forms
{
    partial class DashboardForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblWelcome;
        private System.Windows.Forms.Label lblUserId;
        private System.Windows.Forms.NumericUpDown nudUserId;
        private System.Windows.Forms.Button btnLookupUser;
        private System.Windows.Forms.TextBox txtLookupResult;
        private System.Windows.Forms.Button btnProfile;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.Button btnAdminPanel;
        private System.Windows.Forms.Button btnDiagnostics;
        private System.Windows.Forms.Button btnSaveSnapshot;
        private System.Windows.Forms.Button btnLoadSnapshot;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblWelcome = new System.Windows.Forms.Label();
            this.lblUserId = new System.Windows.Forms.Label();
            this.nudUserId = new System.Windows.Forms.NumericUpDown();
            this.btnLookupUser = new System.Windows.Forms.Button();
            this.txtLookupResult = new System.Windows.Forms.TextBox();
            this.btnProfile = new System.Windows.Forms.Button();
            this.btnSettings = new System.Windows.Forms.Button();
            this.btnAdminPanel = new System.Windows.Forms.Button();
            this.btnDiagnostics = new System.Windows.Forms.Button();
            this.btnSaveSnapshot = new System.Windows.Forms.Button();
            this.btnLoadSnapshot = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.nudUserId)).BeginInit();
            this.SuspendLayout();

            this.lblWelcome.AutoSize = true;
            this.lblWelcome.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblWelcome.Location = new System.Drawing.Point(20, 15);
            this.lblWelcome.Text = "Welcome";

            this.lblUserId.AutoSize = true;
            this.lblUserId.Location = new System.Drawing.Point(20, 60);
            this.lblUserId.Text = "Lookup user profile by ID:";

            this.nudUserId.Location = new System.Drawing.Point(200, 58);
            this.nudUserId.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
            this.nudUserId.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.nudUserId.Value = new decimal(new int[] { 1, 0, 0, 0 });
            this.nudUserId.Width = 80;

            this.btnLookupUser.Location = new System.Drawing.Point(290, 56);
            this.btnLookupUser.Size = new System.Drawing.Size(90, 26);
            this.btnLookupUser.Text = "Lookup";
            this.btnLookupUser.Click += new System.EventHandler(this.btnLookupUser_Click);

            this.txtLookupResult.Location = new System.Drawing.Point(20, 95);
            this.txtLookupResult.Multiline = true;
            this.txtLookupResult.ReadOnly = true;
            this.txtLookupResult.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLookupResult.Size = new System.Drawing.Size(430, 150);

            this.btnProfile.Location = new System.Drawing.Point(20, 260);
            this.btnProfile.Size = new System.Drawing.Size(130, 30);
            this.btnProfile.Text = "My Profile";
            this.btnProfile.Click += new System.EventHandler(this.btnProfile_Click);

            this.btnSettings.Location = new System.Drawing.Point(160, 260);
            this.btnSettings.Size = new System.Drawing.Size(130, 30);
            this.btnSettings.Text = "Settings / Vault";
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);

            this.btnAdminPanel.Location = new System.Drawing.Point(20, 300);
            this.btnAdminPanel.Size = new System.Drawing.Size(130, 30);
            this.btnAdminPanel.Text = "Admin Panel";
            this.btnAdminPanel.Visible = false;
            this.btnAdminPanel.Click += new System.EventHandler(this.btnAdminPanel_Click);

            this.btnDiagnostics.Location = new System.Drawing.Point(160, 300);
            this.btnDiagnostics.Size = new System.Drawing.Size(130, 30);
            this.btnDiagnostics.Text = "Diagnostics";
            this.btnDiagnostics.Visible = false;
            this.btnDiagnostics.Click += new System.EventHandler(this.btnDiagnostics_Click);

            this.btnSaveSnapshot.Location = new System.Drawing.Point(20, 340);
            this.btnSaveSnapshot.Size = new System.Drawing.Size(130, 30);
            this.btnSaveSnapshot.Text = "Save Snapshot";
            this.btnSaveSnapshot.Click += new System.EventHandler(this.btnSaveSnapshot_Click);

            this.btnLoadSnapshot.Location = new System.Drawing.Point(160, 340);
            this.btnLoadSnapshot.Size = new System.Drawing.Size(130, 30);
            this.btnLoadSnapshot.Text = "Load Snapshot";
            this.btnLoadSnapshot.Click += new System.EventHandler(this.btnLoadSnapshot_Click);

            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(470, 390);
            this.Controls.Add(this.lblWelcome);
            this.Controls.Add(this.lblUserId);
            this.Controls.Add(this.nudUserId);
            this.Controls.Add(this.btnLookupUser);
            this.Controls.Add(this.txtLookupResult);
            this.Controls.Add(this.btnProfile);
            this.Controls.Add(this.btnSettings);
            this.Controls.Add(this.btnAdminPanel);
            this.Controls.Add(this.btnDiagnostics);
            this.Controls.Add(this.btnSaveSnapshot);
            this.Controls.Add(this.btnLoadSnapshot);
            this.Text = "c03e - Dashboard";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.DashboardForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudUserId)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
