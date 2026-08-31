# c03e — Vulnerability Map

**c03e.exe** is a deliberately vulnerable Windows thick-client application
(WinForms, .NET 6) with a matching local Flask backend, built for
**authorized security-training practice only** — decompiling, intercepting,
and tampering with it using tools like Fiddler, Burp Suite, Process Hacker,
dnSpy/ILSpy, etc. Run it only inside an isolated lab VM you control. Do not
deploy it on a shared or production network — several features (command
injection, insecure deserialization, unsigned auto-update) can be used to
execute arbitrary code.

Each entry below: **what it is → where it lives in the code → how to find/exploit it with a specific tool → what the fix looks like.**

---

## 1. Hardcoded credentials
- **Where:** `Services/ApiClient.cs` (`BackupServiceApiKey`), `Config/c03e.config.xml` (`ApiKey`, DB connection string), `server/app.py` (`/api/whoami-backdoor`, `/api/config`).
- **Exploit:**
  - **dnSpy/ILSpy**: decompile `c03e.exe`, read the constant string directly out of `ApiClient`.
  - **strings.exe / Process Hacker** (Strings tab on the running process): pull the key straight out of memory/binary without decompiling.
  - Replay the key with **Burp Repeater** / `curl -H "X-Api-Key: ..."` against `/api/whoami-backdoor` to get admin access with zero login.
- **Fix:** no secrets in source or config shipped to clients; use a secrets manager / short-lived tokens issued after real auth.

## 2. Insecure local storage
- **Where:** `Services/LocalStore.cs` → `%APPDATA%\c03e\session.json`.
- **Exploit:** just open the file after logging in — plaintext user ID, role, and session token. **Process Hacker** → double-click c03e.exe → Handles tab, to see the file handle while it's open.
- **Fix:** don't cache anything sensitive client-side; if you must, encrypt with a user-bound key (proper DPAPI, see #4) and add integrity protection.

## 3. Plaintext password extraction
- **Where:** `LoginForm.cs` (`chkRememberMe`) → `LocalStore.SaveSession(..., plaintextPassword)`.
- **Exploit:** check "Remember me", log in, then read `session.json` (`RememberedPassword` field) or `pw.dpapi` (see #4).
- **Fix:** never persist raw passwords; store a refresh token instead, and hash+salt anything password-like that must be compared later.

## 4. DPAPI misuse
- **Where:** `LocalStore.cs` → `ProtectedData.Protect(..., DataProtectionScope.LocalMachine)` with a hardcoded entropy constant.
- **Exploit:** write a tiny C# snippet (or PowerShell `[System.Security.Cryptography.ProtectedData]::Unprotect`) using the SAME hardcoded entropy (visible via dnSpy) and `LocalMachine` scope — decrypts `pw.dpapi` as **any** local user/process, not just the one who saved it.
- **Fix:** use `DataProtectionScope.CurrentUser`, generate entropy per-secret and store it separately (or use Windows Credential Manager / CNG-backed key storage).

## 5. Client-side authentication bypass
- **Where:** `LoginForm.cs` (`localBackdoor` hardcoded check, and the `authenticated` boolean trust) + `server/app.py` `/api/login`.
- **Exploit:**
  - **Burp Suite / Fiddler**: intercept the `/api/login` response, flip `"authenticated": false` → `true`, add any `user_id`/`role` you like — the client accepts it with no signature check.
  - **dnSpy**: patch the IL of `JsonAuthResultOrBackdoor` to always take the "authenticated" branch, or just use the hardcoded `svc_maint` / `M@intWind0w!2024` backdoor found by decompiling.
- **Fix:** issue a signed/opaque server-side session token on success; every subsequent privileged action re-validates that token server-side, never trust a plain boolean in a client-visible response.

## 6. Insecure deserialization
- **Where:** `Services/SessionManager.cs` (`BinaryFormatter` save/load of `.snap` files), bonus server-side `pickle.loads` at `/api/deserialize` in `app.py`.
- **Exploit:** craft a malicious `.snap` with **ysoserial.net** (`-f BinaryFormatter -g TypeConfuseDelegate -o raw -c "calc.exe"`), drop it in `%APPDATA%\c03e\snapshots\`, load it from Dashboard → "Load Snapshot" → code execution. Same idea server-side with a crafted pickle blob POSTed via Burp Repeater.
- **Fix:** never deserialize untrusted data with `BinaryFormatter`/`pickle`; use a safe data format (JSON/Protobuf with strict schemas) and, if you must keep a binary formatter, add a `SerializationBinder` allow-list + HMAC over the file.

## 7. DLL hijacking / binary planting
- **Where:** `Services/UpdateService.cs` (`LoadPlugin` checks CWD before the trusted `Plugins\` folder; `DownloadAndRunUpdateAsync` executes an unsigned download).
- **Exploit:**
  - **Process Monitor (Sysinternals)** or **Process Hacker's** DLL view: watch `c03e.exe`'s module load order to confirm it searches the working directory first.
  - Plant a malicious `ReportExporter.dll` (see `Plugins/README.txt`) in the app's working directory → gets loaded instead of the legitimate plugin.
  - Intercept the `/update/latest` response in **Fiddler/Burp** and swap in your own payload — the client runs it with no signature check.
- **Fix:** load plugins by absolute, hardcoded path only; verify Authenticode signatures on anything downloaded/executed; use `SetDllDirectory`/`LoadLibraryEx` with safe search flags.

## 8. IDOR (Insecure Direct Object Reference)
- **Where:** `server/app.py` → `/api/users/<id>` (GET/PUT), surfaced in `DashboardForm.cs` (`nudUserId` spinner).
- **Exploit:** log in as `jdoe` (non-admin), grab the session token, then in **Burp Repeater** change the URL from `/api/users/1` to `/api/users/3` — you get the admin's full profile (including plaintext password and internal notes) with a token that was never authorized for that ID. Same for the Dashboard's own spinner control — no exploit tooling required, just change the number.
- **Fix:** every object-lookup endpoint must verify the token's owner matches (or is authorized for) the requested resource, server-side, every time.

## 9. Parameter manipulation / local privilege escalation (app-level)
- **Where:** `Config/c03e.config.xml` (`<Role>`), `Services/LocalStore.cs` (`HKCU\Software\c03e\Role`), read by `DashboardForm.RefreshAdminPanelVisibility`.
- **Exploit:** close the app, edit `Config\c03e.config.xml` (Notepad) or `HKCU\Software\c03e` (regedit / **Process Hacker** doesn't edit registry but `regedit`/PowerShell does) and set `Role` to `Admin`. Relaunch — Admin Panel and Diagnostics buttons appear, no server ever consulted.
- **Fix:** never trust a local file/registry value for authorization; re-check role/entitlements against the server on every privileged action.

## 10. Weak encryption cracking
- **Where:** `Services/CryptoHelper.cs` (DES/ECB, hardcoded 8-byte key; XOR "obfuscation" with a hardcoded single byte).
- **Exploit:** pull the key from the binary (dnSpy/strings), or skip that step entirely and just brute-force: DES's 56-bit effective key is crackable offline; the XOR variant is a 1-byte keyspace (256 tries). Decrypt `%APPDATA%\c03e\vault.dat`.
- **Fix:** AES-256-GCM (or similar AEAD) with a properly random, per-install/per-user key derived via a KDF, never hardcoded.

## 11. Memory scraping / credential extraction from memory
- **Where:** `LoginForm.cs` (`_lastPasswordEntered` and the `password` local as plain `string`), `ProfileForm.cs` (`txtToken.Text = _profile.SessionToken`).
- **Exploit:** **Process Hacker** → select `c03e.exe` → Memory tab → Strings (or right-click a private/committed region → "Read/Write memory") while the app is running (even after the login form closes) → search for the password/token substrings.
- **Fix:** use `SecureString`/`Span<byte>` with explicit zeroing for secrets in memory, minimize their lifetime, avoid holding them in fields any longer than necessary.

## 12. Command injection
- **Where:** `Forms/DiagnosticsForm.cs` (`btnRunPing_Click`, string-concatenates `txtHost.Text` into a `cmd.exe /c ping` command line).
- **Exploit:** in the Host field, enter `127.0.0.1 & calc.exe` or `127.0.0.1 && whoami > %TEMP%\out.txt` — reach this screen either as a legitimate admin or via the #9 privilege escalation.
- **Fix:** never build shell command lines via string concatenation; use `ProcessStartInfo.ArgumentList` with validated/allow-listed input, or call a typed API instead of shelling out at all.

## 13. Session token / authentication token replay
- **Where:** `server/app.py` (`make_token` = `MD5(username + static_salt)`, no expiry, no binding).
- **Exploit:** **Fiddler/Burp**: capture one login response, replay the `token` header indefinitely — it never expires and is identical every time that user logs in. You can also precompute a valid token offline for any known username (`md5(username + "c03e_salt_2024")`) without ever touching the login endpoint.
- **Fix:** cryptographically random, server-generated tokens; short expiry + refresh; bind to session metadata (IP/device fingerprint) where appropriate; invalidate on logout.

## 14. Missing TLS validation / certificate-pinning bypass practice
- **Where:** `Program.cs` (`ServicePointManager.ServerCertificateValidationCallback = (...) => true`).
- **Exploit:** point **Fiddler** or **Burp Suite** as a system/WinHTTP proxy — the app will happily talk to the interception proxy over "HTTPS" (or plain HTTP, as configured) with zero certificate complaints, no need to even install Fiddler's/Burp's root CA. Use this to observe/modify every request and response (logins, IDOR calls, the update download).
- **Fix:** never disable certificate validation; if pinning is required, pin to a public key hash (not a full cert) and provide a safe update/rotation path.

## 15. Insecure filesystem / registry permissions
- **Where:** `Services/LocalStore.cs` (`icacls ... /grant Everyone:F` on the data folder, plaintext `HKCU\Software\c03e` values).
- **Exploit:** as ANY other local, unprivileged account on the same machine (e.g., a shared lab/terminal server), browse to `%APPDATA%\...\c03e`'s owner path or use `icacls` / **Process Hacker's** handle/ACL viewer to confirm Everyone:F, then read/replace another user's `session.json` or `vault.dat` directly.
- **Fix:** rely on default per-user ACLs (don't touch them), never grant `Everyone` write access to files containing secrets.

---

## Suggested tool-to-vulnerability workflow

| Tool | Best-fit vulnerabilities |
|---|---|
| **Fiddler / Burp Suite** | #1 (replay hardcoded key), #5 (flip auth boolean), #8 (IDOR via Repeater), #13 (token replay), #14 (no cert validation), #7b (swap update payload) |
| **Process Hacker** | #2/#3 (open file handles), #4 (memory-resident DPAPI key material), #11 (strings in memory), #7a (module/DLL load order), #15 (ACL inspection) |
| **dnSpy / ILSpy** | #1 (hardcoded keys/constants), #5 (patch IL / find backdoor), #10 (extract crypto key), general static analysis of every vuln |
| **ysoserial.net** | #6 (craft BinaryFormatter gadget chain payload) |
| **Sysinternals Process Monitor** | #7a (confirm DLL search order), #9 (watch config/registry reads) |
| **Manual (Notepad/regedit)** | #9 (role tampering), #10 offline decrypt script |

## Setup / build instructions
See `SETUP.md`.
