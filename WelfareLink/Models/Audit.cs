using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WelfareLink.Models
{
    public class Audit
    {
        [Key]
        [Required]
        [StringLength(36)]
        public string AuditID { get; set; }

        [Required]
        [StringLength(36)]
        [ForeignKey("User")]
        public string OfficerID { get; set; }

        [StringLength(500)]
        public string Scope { get; set; }

        [StringLength(4000)] // Assuming Findings could be long
        public string Findings { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; }

        // Navigation properties
        public virtual User Officer { get; set; }
    }
}
