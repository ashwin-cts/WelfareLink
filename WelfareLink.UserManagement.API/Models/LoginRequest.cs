using System.ComponentModel.DataAnnotations;

namespace WelfareLink.UserManagement.API.Models
{
    public class LoginRequest
    {
        [Required]
        public string? Username { get; set; }

        [Required]
        public string? Password { get; set; }

        [Required]
        public string? UserType { get; set; } // Citizen, WelfareOfficer, ProgramManager, ComplianceOfficer, GovernmentAuditor, Admin
    }
}
