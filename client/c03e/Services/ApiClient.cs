using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace c03e.Services
{
    /// <summary>
    /// VULN #1  Hardcoded credentials: a "service account" API key baked
    ///          into the binary, extractable via dnSpy/ILSpy/strings, that
    ///          grants elevated access to the backend regardless of the
    ///          logged-in user.
    /// VULN #5  Client-side authentication bypass: the client decides
    ///          whether the user is "authenticated" purely from a boolean
    ///          in the JSON response body -- it never validates a signed
    ///          token, so replaying/editing the response in Fiddler/Burp
    ///          (or patching the compiled IL with dnSpy) logs you in.
    /// VULN #8  IDOR: profile/user lookups take a raw, client-supplied
    ///          integer ID with no server-side ownership check -- see
    ///          server/app.py for the matching (missing) authorization.
    /// </summary>
    public class ApiClient
    {
        // Base URL points at the local lab backend (server/app.py).
        // Plain HTTP on purpose -- nothing to strip, trivial to view in Fiddler.
        public static string BaseUrl = "http://127.0.0.1:5000";

        // VULN #1: hardcoded "backup admin" service-account key. Pull this
        // string out of the compiled binary and call /api endpoints directly
        // with it (e.g. via curl or Burp Repeater) without ever logging in.
        private const string BackupServiceApiKey = "c03e-svc-8271-BACKUP-ADMIN-KEY";

        private readonly HttpClient _http = new HttpClient();

        public async Task<JObject> LoginAsync(string username, string password)
        {
            var payload = new JObject
            {
                ["username"] = username,
                ["password"] = password
            };

            var content = new StringContent(payload.ToString(), Encoding.UTF8, "application/json");
            var response = await _http.PostAsync($"{BaseUrl}/api/login", content);
            var body = await response.Content.ReadAsStringAsync();

            // VULN #5: whatever the server says, the client believes it.
            // There's no signature, no HMAC, no certificate-bound token check --
            // just trusting a JSON boolean that a proxy can flip in transit.
            return JObject.Parse(body);
        }

        /// <summary>
        /// Fetches a user profile by raw numeric ID. The UI normally passes
        /// the logged-in user's own ID, but nothing stops a caller (or a
        /// modified request in Burp) from passing any other ID -- the
        /// backend does not check that the token's owner matches the
        /// requested ID. See server/app.py:/api/users/&lt;id&gt;.
        /// </summary>
        public async Task<JObject> GetUserProfileAsync(int userId, string sessionToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/api/users/{userId}");
            request.Headers.Add("X-Session-Token", sessionToken);
            var response = await _http.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            return JObject.Parse(body);
        }

        public async Task<JObject> UpdateUserProfileAsync(int userId, string sessionToken, JObject fields)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, $"{BaseUrl}/api/users/{userId}")
            {
                Content = new StringContent(fields.ToString(), Encoding.UTF8, "application/json")
            };
            request.Headers.Add("X-Session-Token", sessionToken);
            var response = await _http.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            return JObject.Parse(body);
        }

        /// <summary>
        /// Calls a privileged endpoint using the hardcoded backup key
        /// instead of a user session -- demonstrates why hardcoded
        /// credentials are a critical, not cosmetic, finding.
        /// </summary>
        public async Task<JObject> CallWithBackdoorKeyAsync(string path)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}{path}");
            request.Headers.Add("X-Api-Key", BackupServiceApiKey);
            var response = await _http.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            return JObject.Parse(body);
        }
    }
}
