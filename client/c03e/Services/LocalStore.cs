using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;
using Newtonsoft.Json;
using c03e.Models;

namespace c03e.Services
{
    /// <summary>
    /// Bundles several storage-related vulnerabilities:
    ///
    /// VULN #2  Insecure local storage: session + profile cached as plaintext JSON
    ///          under %APPDATA%\c03e\session.json.
    /// VULN #3  Plaintext password extraction: "Remember me" writes the raw
    ///          password to disk, no hashing/encryption at all.
    /// VULN #4  DPAPI misuse: uses DataProtectionScope.LocalMachine (not
    ///          CurrentUser) with a constant, hardcoded entropy value, so
    ///          ANY local account/process on the box can call
    ///          ProtectedData.Unprotect with the same entropy and read it --
    ///          defeats the whole point of DPAPI.
    /// VULN #15 Insecure filesystem/registry permissions: the config folder
    ///          is chmod'ed wide open via icacls, and a "license/role" flag
    ///          is stored in HKCU in plaintext, both tamperable by any local
    ///          user or unprivileged malware.
    /// </summary>
    public static class LocalStore
    {
        public static readonly string DataDir =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "c03e");

        public static readonly string SessionFile = Path.Combine(DataDir, "session.json");
        public static readonly string VaultFile = Path.Combine(DataDir, "vault.dat");

        // Hardcoded DPAPI entropy -- defeats the purpose of entropy entirely,
        // since it's baked into the binary and identical for every user/install.
        private static readonly byte[] DpapiEntropy = Encoding.UTF8.GetBytes("c03e-static-entropy");

        public static void EnsureDataDir()
        {
            if (!Directory.Exists(DataDir))
                Directory.CreateDirectory(DataDir);

            // VULN #15a: grant Everyone full control over the app's data folder.
            // Any local, unprivileged user or process can read/modify/replace
            // session tokens, the "vault", or config files belonging to
            // other users on a shared/terminal-server box.
            try
            {
                var psi = new ProcessStartInfo("icacls.exe", $"\"{DataDir}\" /grant Everyone:(OI)(CI)F /T")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                Process.Start(psi);
            }
            catch { /* best-effort in this lab app */ }
        }

        /// <summary>
        /// Saves session + optionally the raw password (Remember Me) as
        /// plaintext JSON. No integrity check, no encryption on the profile
        /// fields, token stored in the clear.
        /// </summary>
        public static void SaveSession(UserProfile profile, string plaintextPasswordIfRemembered)
        {
            EnsureDataDir();

            var record = new
            {
                profile.UserId,
                profile.Username,
                profile.Role,
                profile.SessionToken,          // VULN #13: static/replayable token stored in plain text
                RememberedPassword = plaintextPasswordIfRemembered, // VULN #3: plaintext password on disk
                SavedAtUtc = DateTime.UtcNow
            };

            File.WriteAllText(SessionFile, JsonConvert.SerializeObject(record, Formatting.Indented));

            // Also stash a "misuse" DPAPI-protected copy of the password to show
            // the contrast: even the "protected" copy is trivially decryptable
            // by anything running on the machine because of LocalMachine scope
            // + hardcoded entropy.
            if (!string.IsNullOrEmpty(plaintextPasswordIfRemembered))
            {
                byte[] protectedBytes = ProtectedData.Protect(
                    Encoding.UTF8.GetBytes(plaintextPasswordIfRemembered),
                    DpapiEntropy,
                    DataProtectionScope.LocalMachine); // VULN #4: LocalMachine, not CurrentUser

                File.WriteAllBytes(Path.Combine(DataDir, "pw.dpapi"), protectedBytes);
            }

            // VULN #15b: plaintext role/license flag in the registry, HKCU,
            // no ACL hardening -- any process running as the same user (or
            // admin) can flip Role to "Admin" without ever touching the server.
            using var key = Registry.CurrentUser.CreateSubKey(@"Software\c03e");
            key.SetValue("Role", profile.Role, RegistryValueKind.String);
            key.SetValue("LastUser", profile.Username, RegistryValueKind.String);
        }

        public static dynamic LoadSessionRaw()
        {
            if (!File.Exists(SessionFile)) return null;
            return JsonConvert.DeserializeObject(File.ReadAllText(SessionFile));
        }

        public static string ReadRoleFromRegistry()
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\c03e");
            return key?.GetValue("Role") as string;
        }

        /// <summary>
        /// "Encrypted" vault used for a handful of notes/secrets in the
        /// Settings screen. Actually protected with the broken DES/ECB
        /// helper in CryptoHelper -- see VULN #10.
        /// </summary>
        public static void SaveVault(string secretNote)
        {
            EnsureDataDir();
            string cipher = CryptoHelper.WeakEncrypt(secretNote);
            File.WriteAllText(VaultFile, cipher);
        }

        public static string LoadVault()
        {
            if (!File.Exists(VaultFile)) return string.Empty;
            try
            {
                return CryptoHelper.WeakDecrypt(File.ReadAllText(VaultFile));
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
