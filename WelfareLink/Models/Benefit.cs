using System.ComponentModel.DataAnnotations;

namespace WelfareLink.Models
{
    public class Benefit
    {
        [Key]
        public int BenefitID { get; set; }
        public int ApplicationID { get; set; }
        public string Type { get; set; } = string.Empty;
        public double Amount { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty;

        //navigation property for one-to-many relationship
        //means one benefit can have many disbursements
        public virtual ICollection<Disbursement>? Disbursements { get; set; }
    }
}
