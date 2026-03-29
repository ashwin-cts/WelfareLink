using System.ComponentModel.DataAnnotations;

namespace WelfareLink.Models
{
    public class WelfareApplication
    {
        [Key]
        public int ApplicationID { get; set; }

        [Required]
        public int CitizenID { get; set; }

        [Required]
        public int ProgramID { get; set; }

        [Required]
        public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;

        public DateTime? ReviewDate { get; set; }

        [StringLength(100)]
        public string ReviewedBy { get; set; } = string.Empty;
    }
}
