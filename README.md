# c03e — Vulnerable Thick Client Lab

`c03e.exe` is a deliberately vulnerable Windows desktop application
(C# / .NET 6 WinForms) with a matching local backend (Python/Flask),
built for hands-on practice with thick-client pentesting tools:
**Fiddler, Burp Suite, Process Hacker, dnSpy/ILSpy**, and friends.

It implements 15 critical vulnerabilities drawn from the OWASP Desktop
Application Security Top 10 categories and common real-world thick-client
findings:

1. Hardcoded credentials
2. Insecure local storage
3. Plaintext password extraction
4. DPAPI misuse
5. Client-side authentication bypass
6. Insecure deserialization (BinaryFormatter + bonus server-side pickle)
7. DLL hijacking / binary planting (incl. unsigned auto-update)
8. IDOR (Insecure Direct Object Reference)
9. Parameter manipulation → local privilege escalation (config/registry)
10. Weak encryption (DES/ECB, hardcoded key) — crackable
11. Memory scraping / credential extraction from memory
12. Command injection
13. Session token / authentication token replay
14. Missing TLS validation (no cert pinning)
15. Insecure filesystem / registry permissions

## Structure

```
c03e/
├── client/              C# WinForms source (build → c03e.exe on Windows)
│   ├── c03e.sln
│   └── c03e/
│       ├── Forms/        Login, Dashboard, Profile, Settings, Diagnostics
│       ├── Services/      ApiClient, CryptoHelper, LocalStore, SessionManager, UpdateService, ConfigReader
│       ├── Models/
│       ├── Config/       c03e.config.xml (plaintext secrets, tamperable role)
│       └── Plugins/      DLL-hijack target folder
├── server/               Local Flask backend (IDOR, token replay, etc.)
│   ├── app.py
│   └── requirements.txt
└── docs/
    ├── SETUP.md          Build & run instructions
    └── VULNERABILITIES.md  Full map: vuln → code location → exploit steps per tool → fix
```

## Quick start

See **`docs/SETUP.md`**. Short version:

```
cd server && pip install -r requirements.txt && python app.py
```
Then build `client/c03e.sln` on a Windows machine (Visual Studio or `dotnet build`) and run the resulting `c03e.exe`.

## ⚠️ Important

- This app is **intentionally insecure** — several bugs (command injection,
  insecure deserialization, unsigned auto-update) can lead to real code
  execution. Only run it in an isolated lab VM you control, with no
  sensitive data.
- Not affiliated with any real product or company; "c03e" is a fictional
  corporate name used purely as flavor for the lab scenario.
- Requires Windows to build/run the client (WinForms). The backend is
  cross-platform (Python).
