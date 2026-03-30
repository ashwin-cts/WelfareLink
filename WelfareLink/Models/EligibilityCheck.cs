using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WelfareLink.Models
{
    public class EligibilityCheck
    {
        [Key]
        [Required]
        [StringLength(36)]
        public string CheckID { get; set; }

        [Required]
        [StringLength(36)]
        [ForeignKey("WelfareApplication")]
        public string ApplicationID { get; set; }

        [Required]
        [StringLength(100)]
        public string CriteriaName { get; set; }

        [Required]
        [StringLength(30)]
        public string Result { get; set; }

        [Required]
        public DateTime CheckDate { get; set; }

        [StringLength(36)]
        public string CheckedBy { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }

        // Navigation property
        public virtual WelfareApplication WelfareApplication { get; set; }
    }
}
