namespace c03e.Forms
{
    partial class DiagnosticsForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblWarning;
        private System.Windows.Forms.Label lblHost;
        private System.Windows.Forms.TextBox txtHost;
        private System.Windows.Forms.Button btnRunPing;
        private System.Windows.Forms.TextBox txtOutput;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblWarning = new System.Windows.Forms.Label();
            this.lblHost = new System.Windows.Forms.Label();
            this.txtHost = new System.Windows.Forms.TextBox();
            this.btnRunPing = new System.Windows.Forms.Button();
            this.txtOutput = new System.Windows.Forms.TextBox();
            this.SuspendLayout();

            this.lblWarning.AutoSize = true;
            this.lblWarning.Location = new System.Drawing.Point(20, 15);
            this.lblWarning.ForeColor = System.Drawing.Color.DarkRed;

            this.lblHost.AutoSize = true;
            this.lblHost.Location = new System.Drawing.Point(20, 45);
            this.lblHost.Text = "Host to ping:";

            this.txtHost.Location = new System.Drawing.Point(110, 42);
            this.txtHost.Size = new System.Drawing.Size(220, 23);
            this.txtHost.Text = "127.0.0.1";

            this.btnRunPing.Location = new System.Drawing.Point(340, 41);
            this.btnRunPing.Size = new System.Drawing.Size(90, 26);
            this.btnRunPing.Text = "Run";
            this.btnRunPing.Click += new System.EventHandler(this.btnRunPing_Click);

            this.txtOutput.Location = new System.Drawing.Point(20, 80);
            this.txtOutput.Multiline = true;
            this.txtOutput.ReadOnly = true;
            this.txtOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtOutput.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtOutput.Size = new System.Drawing.Size(410, 200);

            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(450, 300);
            this.Controls.Add(this.lblWarning);
            this.Controls.Add(this.lblHost);
            this.Controls.Add(this.txtHost);
            this.Controls.Add(this.btnRunPing);
            this.Controls.Add(this.txtOutput);
            this.Text = "c03e - Diagnostics (Admin)";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.DiagnosticsForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
