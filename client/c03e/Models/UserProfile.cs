using System;

namespace c03e.Models
{
    // Marked [Serializable] so it can be pushed through BinaryFormatter
    // by SessionManager.cs -- see VULN #6 (insecure deserialization).
    [Serializable]
    public class UserProfile
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }          // VULN #9: role is trusted client-side
        public string Email { get; set; }
        public string Notes { get; set; }
        public string SessionToken { get; set; }  // VULN #11 / #13: lives as plain string in memory
    }
}
