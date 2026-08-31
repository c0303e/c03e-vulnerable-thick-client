using System;
using System.IO;
#pragma warning disable SYSLIB0011 // BinaryFormatter is obsolete/dangerous -- that's the point of this lab.
using System.Runtime.Serialization.Formatters.Binary;
using c03e.Models;

namespace c03e.Services
{
    /// <summary>
    /// VULN #6: Insecure deserialization.
    ///
    /// Saves/loads the current UserProfile using BinaryFormatter, and --
    /// critically -- LoadSnapshot() will happily deserialize ANY .snap file
    /// dropped into %APPDATA%\c03e\snapshots, including one an attacker
    /// crafted with a gadget chain (e.g. via ysoserial.net) to get arbitrary
    /// code execution the moment the file is loaded. There is no type
    /// filtering (no SerializationBinder allow-list) and no integrity check
    /// (no HMAC) on the file before deserializing it.
    /// </summary>
    public static class SessionManager
    {
        public static readonly string SnapshotDir = Path.Combine(LocalStore.DataDir, "snapshots");

        public static string SaveSnapshot(UserProfile profile)
        {
            Directory.CreateDirectory(SnapshotDir);
            string path = Path.Combine(SnapshotDir, $"{profile.Username}.snap");

            var formatter = new BinaryFormatter();
            using var fs = new FileStream(path, FileMode.Create);
            formatter.Serialize(fs, profile); // NOSONAR - intentional for lab
            return path;
        }

        /// <summary>
        /// Loads a "saved session" snapshot. No validation of origin, no
        /// SerializationBinder restricting allowed types, no signature
        /// check -- classic .NET insecure deserialization sink.
        /// </summary>
        public static UserProfile LoadSnapshot(string path)
        {
            if (!File.Exists(path)) return null;

            var formatter = new BinaryFormatter();
            using var fs = new FileStream(path, FileMode.Open);
            return (UserProfile)formatter.Deserialize(fs); // NOSONAR - intentional for lab
        }
    }
}
