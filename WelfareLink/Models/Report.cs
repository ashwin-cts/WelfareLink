using System.ComponentModel.DataAnnotations;

namespace WelfareLink.Models
{
    public class Report
    {
        [Key]
        [Required]
        [StringLength(36)]
        public string ReportID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(50)]
        public string Type { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public DateTime GeneratedDate { get; set; }

        [StringLength(36)]
        public string GeneratedBy { get; set; }

        [StringLength(500)]
        public string FilePath { get; set; }

        [StringLength(30)]
        public string Status { get; set; }

        [StringLength(2000)]
        public string Parameters { get; set; }
    }
}
