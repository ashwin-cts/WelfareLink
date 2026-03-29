using System.ComponentModel.DataAnnotations;

namespace WelfareLink.Models
{
    public class Report
    {
        [Key]
        public int ReportID { get; set; }

        [Required]
        [StringLength(200)]
        public string ReportName { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(100)]
        public string GeneratedBy { get; set; } = string.Empty;

        [StringLength(255)]
        public string FilePath { get; set; } = string.Empty;
    }
}
