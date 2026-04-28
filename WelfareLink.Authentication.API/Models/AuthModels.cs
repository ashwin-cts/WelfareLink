namespace WelfareLink.Authentication.API.Models
{
    public class LoginRequest
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? UserType { get; set; } // Citizen, WelfareOfficer, ProgramManager, ComplianceOfficer, GovernmentAuditor, Admin
    }

    public class LoginResponse
    {
        public string? Token { get; set; }
        public string? Username { get; set; }
        public string? Role { get; set; }
        public string? FullName { get; set; }
        public DateTime ExpiryTime { get; set; }
    }

    public class AuthUser
    {
        public int UserId { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; }
    }
}
