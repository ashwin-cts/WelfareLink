using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using WelfareLink.ProgramManagement.API.Utilities;

namespace WelfareLink.ProgramManagement.API.Models
{
    public class Citizen
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CitizenId { get; set; }

        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Name can only contain letters and spaces.")]
        public string Name { get; set; }

        [DataType(DataType.Date)]
        [MinimumAge(18, ErrorMessage = "Citizen must be at least 18 years old to register.")]
        public DateTime DateOfBirth { get; set; }

        [StringLength(300)]
        public string? Address { get; set; }

        [StringLength(50)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Mobile number must be exactly 10 digits.")]
        public string? ContactInfo { get; set; }

        [StringLength(50)]
        public string? Status { get; set; }

        [StringLength(20)]
        public string? Gender { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [JsonIgnore]
        public virtual ICollection<CitizenDocument>? CitizenDocuments { get; set; }
    }
}