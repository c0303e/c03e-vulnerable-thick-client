using System;
using System.Windows.Forms;
using c03e.Forms;

namespace c03e
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // VULN #14: TLS certificate validation disabled application-wide.
            // Any MITM proxy (Fiddler, Burp Suite) can intercept and modify traffic
            // without needing the app to trust a custom root CA.
            System.Net.ServicePointManager.ServerCertificateValidationCallback =
                (sender, cert, chain, sslPolicyErrors) => true;

            // Resolves any assembly sitting next to c03e.exe that .NET's default
            // loader wouldn't otherwise find (it only auto-probes assemblies
            // listed in c03e.deps.json). This mirrors how many real thick-client
            // "plugin" systems are built -- and is what makes the insecure
            // deserialization PoC (VULN #6, see docs/VULNERABILITIES.md) work
            // when the malicious payload's type lives in an assembly that isn't
            // already loaded by the process.
            System.Runtime.Loader.AssemblyLoadContext.Default.Resolving += (context, name) =>
            {
                string candidate = System.IO.Path.Combine(AppContext.BaseDirectory, name.Name + ".dll");
                if (System.IO.File.Exists(candidate))
                    return context.LoadFromAssemblyPath(candidate);
                return null;
            };

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoginForm());
        }
    }
}
