using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WelfareLink.Models
{
    public class Benefit
    {
        [Key]
        [Required]
        [StringLength(36)]
        public string BenefitID { get; set; }

        [Required]
        [StringLength(36)]
        [ForeignKey("WelfareApplication")]
        public string ApplicationID { get; set; }

        [Required]
        [StringLength(36)]
        [ForeignKey("WelfareProgram")]
        public string ProgramID { get; set; }

        [Required]
        [StringLength(50)]
        public string BenefitType { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        // Navigation properties
        public virtual WelfareApplication WelfareApplication { get; set; }
        public virtual WelfareProgram WelfareProgram { get; set; }
        public virtual ICollection<Disbursement> Disbursements { get; set; }
    }
}
