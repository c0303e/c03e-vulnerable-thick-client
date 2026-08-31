namespace c03e.Forms
{
    partial class LoginForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.CheckBox chkRememberMe;
        private System.Windows.Forms.Button btnLogin;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblUsername = new System.Windows.Forms.Label();
            this.lblPassword = new System.Windows.Forms.Label();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.chkRememberMe = new System.Windows.Forms.CheckBox();
            this.btnLogin = new System.Windows.Forms.Button();
            this.SuspendLayout();

            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(30, 20);
            this.lblTitle.Text = "c03e Corporate Portal";

            this.lblUsername.AutoSize = true;
            this.lblUsername.Location = new System.Drawing.Point(30, 80);
            this.lblUsername.Text = "Username";

            this.txtUsername.Location = new System.Drawing.Point(30, 100);
            this.txtUsername.Size = new System.Drawing.Size(260, 23);

            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(30, 135);
            this.lblPassword.Text = "Password";

            this.txtPassword.Location = new System.Drawing.Point(30, 155);
            this.txtPassword.Size = new System.Drawing.Size(260, 23);
            this.txtPassword.UseSystemPasswordChar = true;

            this.chkRememberMe.AutoSize = true;
            this.chkRememberMe.Location = new System.Drawing.Point(30, 190);
            this.chkRememberMe.Text = "Remember me on this device";

            this.btnLogin.Location = new System.Drawing.Point(30, 225);
            this.btnLogin.Size = new System.Drawing.Size(260, 32);
            this.btnLogin.Text = "Sign In";
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);

            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(320, 280);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblUsername);
            this.Controls.Add(this.txtUsername);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.chkRememberMe);
            this.Controls.Add(this.btnLogin);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "c03e - Sign In";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
