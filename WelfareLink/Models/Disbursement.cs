using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WelfareLink.Models
{
    public class Disbursement
    {
        [Key]
        [Required]
        [StringLength(36)]
        public string DisbursementID { get; set; }

        [Required]
        [StringLength(36)]
        [ForeignKey("Benefit")]
        public string BenefitID { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime DisbursementDate { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; }

        [StringLength(100)]
        public string TransactionReference { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }

        // Navigation property
        public virtual Benefit Benefit { get; set; }
    }
}
