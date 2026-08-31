using System;
using System.Windows.Forms;
using c03e.Models;
using c03e.Services;

namespace c03e.Forms
{
    public partial class LoginForm : Form
    {
        private readonly ApiClient _api = new ApiClient();

        // VULN #11: password lives as a plain, immutable System.String for
        // the lifetime of the form -- it is NOT wiped from memory and is
        // trivially recoverable with Process Hacker's "Strings" feature on
        // the c03e.exe process while this form is open (or even after, until
        // the GC eventually collects/reuses the memory page).
        private string _lastPasswordEntered;

        public LoginForm()
        {
            InitializeComponent();
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Text;
            _lastPasswordEntered = password;

            // VULN #5a: local backdoor -- a hardcoded bypass that skips the
            // server entirely. Findable by decompiling c03e.exe with dnSpy/
            // ILSpy and reading this method, or by patching the compiled IL
            // to always branch into the "authenticated" path.
            bool localBackdoor = (username == "svc_maint" && password == "M@intWind0w!2024");

            JsonAuthResultOrBackdoor(username, password, localBackdoor);
        }

        private async void JsonAuthResultOrBackdoor(string username, string password, bool localBackdoor)
        {
            try
            {
                UserProfile profile;

                if (localBackdoor)
                {
                    profile = new UserProfile
                    {
                        UserId = 1,
                        Username = username,
                        FullName = "Maintenance Service Account",
                        Role = "Admin",
                        Email = "svc_maint@c03e.local",
                        SessionToken = "LOCALBYPASS-" + Guid.NewGuid().ToString("N").Substring(0, 8)
                    };
                }
                else
                {
                    var result = await _api.LoginAsync(username, password);

                    // VULN #5b: the ONLY thing gating access is this boolean
                    // read out of the HTTP response body. Intercept the
                    // response in Fiddler/Burp and change "authenticated":
                    // false to true (with any username) and this branch
                    // logs you straight in -- the client never validates a
                    // signed token or asks the server again to confirm.
                    bool authenticated = result.Value<bool?>("authenticated") ?? false;
                    if (!authenticated)
                    {
                        MessageBox.Show(this, "Invalid credentials.", "c03e",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    profile = new UserProfile
                    {
                        UserId = result.Value<int?>("user_id") ?? 0,
                        Username = username,
                        FullName = result.Value<string>("full_name"),
                        Role = result.Value<string>("role"),
                        Email = result.Value<string>("email"),
                        SessionToken = result.Value<string>("token")
                    };
                }

                if (chkRememberMe.Checked)
                {
                    // VULN #3: writes the RAW password to disk on request.
                    LocalStore.SaveSession(profile, password);
                }
                else
                {
                    LocalStore.SaveSession(profile, null);
                }

                var dashboard = new DashboardForm(profile);
                dashboard.Show();
                Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Login failed: {ex.Message}", "c03e",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
