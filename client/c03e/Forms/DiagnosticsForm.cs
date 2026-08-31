using System;
using System.Diagnostics;
using System.Windows.Forms;
using c03e.Services;

namespace c03e.Forms
{
    public partial class DiagnosticsForm : Form
    {
        public DiagnosticsForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// VULN #12: Command injection. The "host" field is concatenated
        /// directly into a cmd.exe command line with no validation and no
        /// use of ProcessStartInfo.ArgumentList. Input like:
        ///     127.0.0.1 & calc.exe
        ///     127.0.0.1 && whoami > out.txt
        /// breaks out of the intended ping command entirely.
        /// </summary>
        private void btnRunPing_Click(object sender, EventArgs e)
        {
            string host = txtHost.Text;

            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c ping -n 2 " + host, // <-- unsanitized string concatenation
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using var proc = Process.Start(psi);
                string output = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();
                txtOutput.Text = output;
            }
            catch (Exception ex)
            {
                txtOutput.Text = "Error: " + ex.Message;
            }
        }

        // Admin-only screen already gated purely by the local, tamperable
        // Role flag (see DashboardForm.RefreshAdminPanelVisibility / VULN #9)
        // -- so reaching this form at all may itself be the result of a
        // privilege-escalation bypass rather than legitimate admin access.
        private void DiagnosticsForm_Load(object sender, EventArgs e)
        {
            lblWarning.Text = $"Running as: {Environment.UserName} | Role (local): {ConfigReader.GetRole()}";
        }
    }
}
