using System;
using System.IO;
using System.Windows.Forms;
using c03e.Models;
using c03e.Services;

namespace c03e.Forms
{
    public partial class DashboardForm : Form
    {
        private readonly UserProfile _profile;
        private readonly ApiClient _api = new ApiClient();

        public DashboardForm(UserProfile profile)
        {
            _profile = profile;
            InitializeComponent();
        }

        private void DashboardForm_Load(object sender, EventArgs e)
        {
            lblWelcome.Text = $"Welcome, {_profile.FullName} ({_profile.Username})";
            nudUserId.Value = _profile.UserId > 0 ? _profile.UserId : 1;

            // VULN #9: privilege check reads the Role from a local, freely
            // tamperable source (app config file / registry) rather than
            // re-validating with the server on every sensitive action.
            RefreshAdminPanelVisibility();
        }

        private void RefreshAdminPanelVisibility()
        {
            string configRole = ConfigReader.GetRole();      // from c03e.config.xml
            string registryRole = LocalStore.ReadRoleFromRegistry(); // from HKCU

            bool isAdmin = string.Equals(configRole, "Admin", StringComparison.OrdinalIgnoreCase)
                         || string.Equals(registryRole, "Admin", StringComparison.OrdinalIgnoreCase)
                         || string.Equals(_profile.Role, "Admin", StringComparison.OrdinalIgnoreCase);

            btnAdminPanel.Visible = isAdmin;
            btnDiagnostics.Visible = isAdmin;
        }

        // VULN #8: IDOR. The spinner next to "Lookup user profile" lets the
        // caller request ANY numeric user ID -- there's no check that it
        // matches the logged-in user's own ID, and the backend
        // (/api/users/<id>) doesn't enforce ownership either.
        private async void btnLookupUser_Click(object sender, EventArgs e)
        {
            int requestedId = (int)nudUserId.Value;
            var result = await _api.GetUserProfileAsync(requestedId, _profile.SessionToken);
            txtLookupResult.Text = result.ToString(Newtonsoft.Json.Formatting.Indented);
        }

        private void btnAdminPanel_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this,
                "Admin panel unlocked via local Role flag -- this should have required " +
                "a fresh server-side authorization check, not a local config/registry read.",
                "c03e Admin", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnDiagnostics_Click(object sender, EventArgs e)
        {
            new DiagnosticsForm().Show();
        }

        private void btnProfile_Click(object sender, EventArgs e)
        {
            new ProfileForm(_profile).Show();
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            new SettingsForm().Show();
        }

        private void btnSaveSnapshot_Click(object sender, EventArgs e)
        {
            // VULN #6 hook: writes a BinaryFormatter snapshot the user can
            // later "restore" -- and so can an attacker who plants a
            // malicious .snap file with the same name/location.
            string path = SessionManager.SaveSnapshot(_profile);
            MessageBox.Show(this, $"Snapshot saved to:\n{path}", "c03e");
        }

        private void btnLoadSnapshot_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                InitialDirectory = SessionManager.SnapshotDir,
                Filter = "c03e snapshot (*.snap)|*.snap"
            };
            if (ofd.ShowDialog(this) == DialogResult.OK)
            {
                var loaded = SessionManager.LoadSnapshot(ofd.FileName);
                MessageBox.Show(this, $"Loaded snapshot for: {loaded?.Username}", "c03e");
            }
        }
    }
}
