using System;
using System.IO;
using System.Xml.Linq;

namespace c03e.Services
{
    /// <summary>
    /// VULN #9 (config half): c03e.config.xml sits next to the executable
    /// in plaintext, contains a DB connection string with embedded
    /// credentials and a "Role" the dashboard trusts to unlock admin
    /// features. Any local user with write access to the install folder
    /// (or the app running from a user-writable path) can edit this file
    /// with Notepad and hand themselves admin, no exploit tooling required.
    /// </summary>
    public static class ConfigReader
    {
        private static string ConfigPath =>
            Path.Combine(AppContext.BaseDirectory, "Config", "c03e.config.xml");

        public static string GetRole()
        {
            try
            {
                var doc = XDocument.Load(ConfigPath);
                return doc.Root?.Element("Role")?.Value;
            }
            catch
            {
                return "User";
            }
        }

        public static string GetDbConnectionString()
        {
            try
            {
                var doc = XDocument.Load(ConfigPath);
                return doc.Root?.Element("DbConnectionString")?.Value;
            }
            catch
            {
                return null;
            }
        }

        public static string GetApiKey()
        {
            try
            {
                var doc = XDocument.Load(ConfigPath);
                return doc.Root?.Element("ApiKey")?.Value;
            }
            catch
            {
                return null;
            }
        }
    }
}
