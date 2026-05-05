using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WelfareLink.ProgramManagement.API.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; }

        [Required]
        [StringLength(100)]
        // Added Password Strength Validation
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string Password { get; set; }

        [Required]
        [StringLength(50)]
        public string Role { get; set; } // Citizen, WelfareOfficer, ProgramManager, Admin

        [StringLength(100)]
        // Added Name Validation
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Full Name can only contain letters and spaces.")]
        public string? FullName { get; set; }

        [StringLength(100)]
        // Added Email Validation
        [EmailAddress(ErrorMessage = "Please provide a valid email address format.")]
        public string? Email { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? CitizenId { get; set; }

        [ForeignKey("CitizenId")]
        public virtual Citizen? Citizen { get; set; }
    }
}