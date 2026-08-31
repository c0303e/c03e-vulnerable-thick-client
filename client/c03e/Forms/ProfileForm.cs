using System;
using System.Windows.Forms;
using c03e.Models;
using c03e.Services;

namespace c03e.Forms
{
    public partial class ProfileForm : Form
    {
        private readonly UserProfile _profile;

        public ProfileForm(UserProfile profile)
        {
            _profile = profile;
            InitializeComponent();
        }

        private void ProfileForm_Load(object sender, EventArgs e)
        {
            txtUsername.Text = _profile.Username;
            txtFullName.Text = _profile.FullName;
            txtEmail.Text = _profile.Email;
            txtRole.Text = _profile.Role;

            // VULN #11/#13: the session token is displayed (and was already
            // sitting in memory as a plain string on the UserProfile
            // object) -- open Process Hacker, find c03e.exe, use the
            // Strings tab on its memory to recover this token without ever
            // touching this form.
            txtToken.Text = _profile.SessionToken;
        }

        private void btnShowStoredSecrets_Click(object sender, EventArgs e)
        {
            // Reads back whatever LocalStore wrote to disk -- demonstrates
            // VULN #2/#3 (plaintext session/password on disk) from inside
            // the app itself, so learners can see what a local attacker
            // would find at %APPDATA%\c03e\session.json.
            dynamic raw = LocalStore.LoadSessionRaw();
            txtStoredSecrets.Text = raw != null
                ? Newtonsoft.Json.JsonConvert.SerializeObject(raw, Newtonsoft.Json.Formatting.Indented)
                : "(no session.json found yet -- log in with 'Remember me' checked first)";
        }
    }
}
