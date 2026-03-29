using System.ComponentModel.DataAnnotations;

namespace WelfareLink.Models
{
    public class Benefit
    {
        [Key]
        public int BenefitID { get; set; }

        [Required]
        public double Amount { get; set; }

        [Required]
        public int ApplicationID { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string Status { get; set; } = string.Empty;

        [Required]
        public string Type { get; set; } = string.Empty;

        // Navigation property
        public virtual ICollection<Disbursement> Disbursements { get; set; } = new List<Disbursement>();
    }
}
