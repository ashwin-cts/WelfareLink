using System.ComponentModel.DataAnnotations;

namespace WelfareLink.Models
{
    public class ComplainceRecord
    {
        public int RecordID { get; set; }

        public int? RaisedByUserId { get; set; }
        public string? RaisedByUserName { get; set; }

        [Required, Display(Name = "Entity Type")]
        public string EntityType { get; set; } = string.Empty;

        [Display(Name = "Entity ID")]
        public int EntityId { get; set; }

        [Required, Display(Name = "Violation Type")]
        public string ViolationType { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public string Status { get; set; } = "Open";

        [Display(Name = "Date Raised")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Date Resolved")]
        public DateTime? ResolvedDate { get; set; }

        public int? ResolvedByUserId { get; set; }
        public string? ResolvedByUserName { get; set; }
        public string? Notes { get; set; }

        // Application context for cascading form
        public int? ApplicationId { get; set; }
        public string? ApplicationName { get; set; }
        public string? CitizenName { get; set; }
        public List<BenefitOption> BenefitsList { get; set; } = new();
        public List<DisbursementOption> DisbursementsList { get; set; } = new();
        public List<EligibilityCheckOption> EligibilityChecksList { get; set; } = new();
    }

    public class BenefitOption
    {
        public int BenefitID { get; set; }
        public string Type { get; set; } = string.Empty;
        public double Amount { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class DisbursementOption
    {
        public int DisbursementID { get; set; }
        public int BenefitID { get; set; }
        public double Amount { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class EligibilityCheckOption
    {
        public int CheckID { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
