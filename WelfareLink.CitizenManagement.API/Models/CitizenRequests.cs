using System.ComponentModel.DataAnnotations;
using WelfareLink.CitizenManagement.API.Utilities;
namespace WelfareLink.CitizenManagement.API.Models
{
    public class CitizenApplyRequest
    {
        public int CitizenID { get; set; }
        public int ProgramID { get; set; }
        public int[]? SelectedDocumentIds { get; set; }
    }

    public class CreateCitizenRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9\s])\S{8,}$",
            ErrorMessage = "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Name can only contain letters and spaces.")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress(ErrorMessage = "Please provide a valid email address format.")]
        public string Email { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [MinimumAge(18, ErrorMessage = "Citizen must be at least 18 years old to register.")]
        public DateTime DateOfBirth { get; set; }

        [StringLength(300)]
        public string Address { get; set; } = string.Empty;

        [StringLength(50)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Mobile number must be exactly 10 digits.")]
        public string ContactInfo { get; set; } = string.Empty;

        public string Gender { get; set; } = string.Empty;
    }
}
