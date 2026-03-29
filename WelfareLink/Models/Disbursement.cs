using System.ComponentModel.DataAnnotations;

namespace WelfareLink.Models
{
    public class Disbursement
    {
        [Key]
        public int DisbursementID { get; set; }

        [Required]
        public double Amount { get; set; }

        [Required]
        public int BenefitID { get; set; }

        [Required]
        public int CitizenID { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public int OfficerID { get; set; }

        [Required]
        public string Status { get; set; } = string.Empty;

        // Navigation property
        public virtual Benefit? Benefit { get; set; }
    }
}
