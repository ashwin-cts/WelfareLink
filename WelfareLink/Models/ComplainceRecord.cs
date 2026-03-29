using System.ComponentModel.DataAnnotations;

namespace WelfareLink.Models
{
    public class ComplainceRecord
    {
        [Key]
        public int RecordID { get; set; }

        [Required]
        public int CitizenID { get; set; }

        [Required]
        [StringLength(200)]
        public string ComplianceType { get; set; } = string.Empty;

        [Required]
        public DateTime CheckDate { get; set; } = DateTime.UtcNow;

        [Required]
        public bool IsCompliant { get; set; }

        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;
    }
}
