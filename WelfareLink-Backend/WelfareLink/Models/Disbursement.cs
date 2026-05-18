using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WelfareLink.Models
{
    public class Disbursement
    {
        [Key]
        public int DisbursementID { get; set; }

        [ForeignKey("Benefit")]
        public int BenefitID { get; set; }
        public int CitizenID { get; set; }
        public int OfficerID { get; set; }
        public double Amount { get; set; }  // Amount disbursed in this transaction
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty;

        //navigation property
        //means one disbursement belongs to one benefit
        public virtual Benefit? Benefit { get; set; }

    }
}
