# c03e — Setup & Build

> ⚠️ **Lab use only.** This app contains real, working vulnerabilities
> (command injection, insecure deserialization → RCE, unsigned
> auto-update → RCE). Build and run it only inside an isolated VM/lab
> network with no sensitive data and no access to production systems.

## Requirements

- Windows 10/11 (WinForms only runs on Windows)
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) (Desktop workload)
- Visual Studio 2022 (Community is fine) **or** just the `dotnet` CLI
- Python 3.9+ for the backend
- Fiddler Classic / Fiddler Everywhere, Burp Suite Community, and
  Process Hacker (or its maintained fork, System Informer) installed on
  the same lab VM

## 1. Run the backend

```
cd server
pip install -r requirements.txt
python app.py
```

Confirm it's up:

```
curl http://127.0.0.1:5000/api/config
```

It listens on `http://127.0.0.1:5000` — plain HTTP on purpose, so there's
nothing to strip before Fiddler/Burp can read it. Leave this running in a
separate terminal.

## 2. Build the client

### Option A — Visual Studio
1. Open `client/c03e.sln`.
2. Let NuGet restore (`System.Data.SQLite.Core`, `Newtonsoft.Json`,
   `Microsoft.Win32.Registry`, `System.Runtime.Serialization.Formatters`).
3. Build → Debug or Release. Output: `client/c03e/bin/Debug/net6.0-windows/c03e.exe`.

### Option B — CLI
```
cd client\c03e
dotnet restore
dotnet build -c Release
```
Output: `client\c03e\bin\Release\net6.0-windows\c03e.exe`.

Make sure `Config\c03e.config.xml` is present next to the built `c03e.exe`
(the `.csproj` already copies it on build).

## 3. Log in

Seeded accounts (see `server/app.py`):

| Username | Password | Role |
|---|---|---|
| jdoe | Summer2024! | User |
| asmith | Passw0rd123 | User |
| admin | AdminPass!2024 | Admin |

Local backdoor (no server involved — found by decompiling): `svc_maint` / `M@intWind0w!2024`.

## Troubleshooting (issues found during lab testing)

- **`net6.0-windows` / NU1202 package errors on build**: this repo now
  targets `net8.0-windows` and pins compatible package versions. If you
  still see this, make sure you're on .NET 8 SDK (`dotnet --version`) and
  re-run `dotnet nuget locals all --clear` before building again.
- **`MSB3027`/`MSB3021` "file is locked by another process" on rebuild**:
  you have a stale `c03e.exe` still running (often from a previous crash
  dialog left open). Run `taskkill /F /IM c03e.exe /T`, confirm with
  `tasklist | findstr c03e`, then rebuild.
- **Config/registry role tampering (VULN #9) doesn't unlock the Admin
  Panel**: double-check you edited the `c03e.config.xml` inside
  `bin\Release\net8.0-windows\Config\`, not the one under the source tree
  (`client\c03e\Config\`) -- only the compiled-output copy is actually read
  at runtime.
- **Fiddler shows nothing for `127.0.0.1` traffic**: Fiddler doesn't
  capture loopback traffic by default. Either enable "Allow capturing of
  local traffic" in Fiddler's connection options, or change
  `ApiClient.BaseUrl` to `http://localhost:5000` and rebuild.
- **A PUT request via Fiddler/Burp Composer returns 200 but nothing
  changed**: your `Content-Length` header didn't match the actual body
  size, so Flask silently parsed an empty/truncated JSON body. Either
  remove the `Content-Length` line entirely (Fiddler recalculates it) or
  don't set it by hand.
- **ysoserial.net payloads throw "Invalid BinaryFormatter stream" on
  load**: classic gadgets like `TypeConfuseDelegate` rely on a reflection
  trick .NET patched starting with .NET Core 3.0, so they're unreliable
  against a .NET 8 target. See `attacker-tools/EvilSerializer/` for a
  hand-built gadget that's guaranteed compatible instead.
- **Custom gadget throws "Unable to find assembly 'EvilSerializer...'"**:
  .NET (Core/5+) doesn't auto-probe the app directory for arbitrary
  assemblies the way classic .NET Framework did. `Program.cs` now
  registers an `AssemblyLoadContext.Default.Resolving` handler so any
  `.dll` sitting next to `c03e.exe` gets picked up -- make sure you're on
  the updated `Program.cs` and that `EvilSerializer.dll` is physically
  copied next to `c03e.exe`.

## 4. Point your interception proxy at it

- **Fiddler**: enable "Capture Traffic", set it as the system proxy (or
  WinHTTP proxy — WinForms `HttpClient` respects the system proxy by
  default). Since `Program.cs` disables certificate validation, you don't
  even need to trust Fiddler's root CA for HTTPS variants.
- **Burp Suite**: same idea — set Burp as the system proxy, open Proxy →
  HTTP history, then try IDOR/auth-bypass/token-replay from `VULNERABILITIES.md`
  using Repeater.
- **Process Hacker**: run it elevated, find `c03e.exe` in the process list,
  use the Memory (Strings) tab for #4/#11, Handles tab for #2/#3/#15,
  Modules tab for #7.

## Resetting lab state

The backend's "database" is in-memory (`USERS` dict in `app.py`) — restart
`app.py` to reset it. Client-side state lives at
`%APPDATA%\c03e\` and `HKCU\Software\c03e` — delete the folder and the
registry key to reset the client.

## Full vulnerability map

See `VULNERABILITIES.md` for all 15 vulnerabilities, where they live in
the code, and how to exploit each one with a specific tool.
