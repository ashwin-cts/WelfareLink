using System.ComponentModel.DataAnnotations;

namespace WelfareLink.Models
{
    public class EligibilityCheck
    {
        [Key]
        public int CheckID { get; set; }

        [Required]
        public int CitizenID { get; set; }

        [Required]
        public int ProgramID { get; set; }

        [Required]
        public DateTime CheckDate { get; set; } = DateTime.UtcNow;

        [Required]
        public bool IsEligible { get; set; }

        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;
    }
}
