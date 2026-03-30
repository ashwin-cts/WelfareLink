using System.ComponentModel.DataAnnotations;

namespace WelfareLink.Models
{
    public class ComplianceRecord
    {
        [Key]
        [Required]
        [StringLength(36)]
        public string ComplianceID { get; set; }

        [Required]
        [StringLength(36)]
        public string EntityID { get; set; }

        [Required]
        [StringLength(30)]
        public string Type { get; set; }

        [Required]
        [StringLength(30)]
        public string Result { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [StringLength(2000)]
        public string Notes { get; set; }
    }
}
