using System;
using System.Windows.Forms;
using c03e.Services;

namespace c03e.Forms
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            txtVault.Text = LocalStore.LoadVault();
        }

        // VULN #10: "secure note" is protected with the broken DES/ECB +
        // hardcoded-key helper in CryptoHelper.cs. Pull the key from the
        // binary (dnSpy/strings) and decrypt %APPDATA%\c03e\vault.dat
        // offline, or just brute force the 56-bit DES keyspace.
        private void btnSaveVault_Click(object sender, EventArgs e)
        {
            LocalStore.SaveVault(txtVault.Text);
            MessageBox.Show(this, "Saved to vault.dat (weakly \"encrypted\" -- see CryptoHelper.cs).", "c03e");
        }
    }
}
