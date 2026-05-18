using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WelfareLink.Models
{
    public class Benefit
    {
        [Key]
        public int BenefitID { get; set; }

        [ForeignKey("WelfareApplication")]
        [Required(ErrorMessage = "Application ID is required")]
        [Display(Name = "Application ID")]
        public int ApplicationID { get; set; }

        public string Type { get; set; } = string.Empty;
        public double Amount { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty;

        //navigation property for one-to-many relationship
        //means one benefit can have many disbursements
        public virtual ICollection<Disbursement>? Disbursements { get; set; }

        // Navigation property
        // One Benefit belongs to one WelfareApplication
        public virtual WelfareApplication? WelfareApplication { get; set; }
    }
}
