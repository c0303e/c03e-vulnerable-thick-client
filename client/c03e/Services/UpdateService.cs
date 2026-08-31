using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace c03e.Services
{
    /// <summary>
    /// VULN #7  DLL hijacking / binary planting: plugins are loaded by
    ///          filename only (Assembly.LoadFrom(name)), and the search
    ///          logic checks the current working directory BEFORE the
    ///          app's own "trusted" Plugins folder. Any process that can
    ///          write a file named like a real plugin into the CWD the app
    ///          is launched from (a shared Downloads folder, a USB drive,
    ///          a writable install directory) gets its DLL loaded and
    ///          executed in the context of c03e.exe.
    /// Also demonstrates unsigned-update binary planting: the "check for
    /// update" flow downloads and executes a file over HTTP with no
    /// signature/hash verification whatsoever.
    /// </summary>
    public class UpdateService
    {
        private readonly HttpClient _http = new HttpClient();

        /// <summary>
        /// VULN #7a: Loads a "plugin" DLL by bare filename. .NET/Windows
        /// assembly probing plus this code's own directory search order
        /// means a malicious DLL placed in the working directory (rather
        /// than the real Plugins\ folder next to c03e.exe) gets loaded
        /// first. Great target for Process Monitor / Process Hacker to
        /// watch the LoadLibrary search order.
        /// </summary>
        public Assembly LoadPlugin(string pluginFileName)
        {
            // Insecure: checks the CURRENT WORKING DIRECTORY first, which is
            // often not the same as the app's install directory (e.g. when
            // launched via a shortcut with a different "Start in" path, or
            // from a shared folder). This is the classic DLL hijack pattern.
            string cwdPath = Path.Combine(Directory.GetCurrentDirectory(), pluginFileName);
            if (File.Exists(cwdPath))
            {
                return Assembly.LoadFrom(cwdPath); // <-- attacker-plantable
            }

            string trustedPath = Path.Combine(AppContext.BaseDirectory, "Plugins", pluginFileName);
            if (File.Exists(trustedPath))
            {
                return Assembly.LoadFrom(trustedPath);
            }

            throw new FileNotFoundException("Plugin not found", pluginFileName);
        }

        /// <summary>
        /// VULN #7b: Binary planting via unsigned auto-update. Downloads
        /// whatever the server hands back and executes it directly -- no
        /// Authenticode signature check, no hash/checksum pinning, plain
        /// HTTP by default (see ApiClient.BaseUrl), so a MITM (Fiddler/Burp)
        /// can swap the payload in transit.
        /// </summary>
        public async Task<string> DownloadAndRunUpdateAsync(string updateUrl)
        {
            byte[] bytes = await _http.GetByteArrayAsync(updateUrl);

            string tempPath = Path.Combine(Path.GetTempPath(), "c03e_update.exe");
            File.WriteAllBytes(tempPath, bytes);

            // No signature check, no hash comparison against a known-good
            // value, just... run it.
            Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true });
            return tempPath;
        }
    }
}
