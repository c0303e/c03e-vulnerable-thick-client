using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace c03e.Services
{
    /// <summary>
    /// VULN #10: Weak encryption, crackable by design.
    ///
    /// Problems on purpose:
    ///  - Hardcoded static key + IV, identical for every install (extractable
    ///    from the compiled binary with dnSpy/ILSpy/strings).
    ///  - DES in ECB mode (no IV, patterns leak, trivially brute-forced --
    ///    56-bit effective keyspace is crackable offline in minutes/hours).
    ///  - Same key used to "encrypt" the local vault AND the remember-me
    ///    token, so cracking it once compromises everything.
    /// </summary>
    public static class CryptoHelper
    {
        // Hardcoded key -- pull this out of the compiled assembly with dnSpy
        // or `strings c03e.exe` to fully break the "encryption".
        private static readonly byte[] StaticKey = Encoding.ASCII.GetBytes("C03eK3y!"); // 8 bytes = DES key

        public static string WeakEncrypt(string plainText)
        {
            using var des = DES.Create();
            des.Key = StaticKey;
            des.Mode = CipherMode.ECB;   // no IV -> identical plaintext blocks = identical ciphertext blocks
            des.Padding = PaddingMode.PKCS7;

            using var encryptor = des.CreateEncryptor();
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            return Convert.ToBase64String(cipherBytes);
        }

        public static string WeakDecrypt(string cipherTextBase64)
        {
            using var des = DES.Create();
            des.Key = StaticKey;
            des.Mode = CipherMode.ECB;
            des.Padding = PaddingMode.PKCS7;

            using var decryptor = des.CreateDecryptor();
            byte[] cipherBytes = Convert.FromBase64String(cipherTextBase64);
            byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            return Encoding.UTF8.GetString(plainBytes);
        }

        /// <summary>
        /// "Obfuscation" some devs mistake for encryption -- single-byte XOR.
        /// Used for the local SQLite vault. Breakable via frequency analysis
        /// or simply brute-forcing 256 possible keys.
        /// </summary>
        public static byte[] XorObfuscate(byte[] data, byte key = 0x5A)
        {
            byte[] outBytes = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
                outBytes[i] = (byte)(data[i] ^ key);
            return outBytes;
        }
    }
}
