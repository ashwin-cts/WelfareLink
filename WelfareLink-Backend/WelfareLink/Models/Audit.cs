using System.ComponentModel.DataAnnotations;

namespace WelfareLink.Models
{
    public class Audit
    {
        public int AuditID { get; set; }

        [Display(Name = "Program ID")]
        public int? ProgramID { get; set; }
        public string? ProgramTitle { get; set; }

        public int AuditedByUserId { get; set; }
        public string? AuditedByUserName { get; set; }

        [Display(Name = "Audit Date")]
        public DateTime AuditDate { get; set; }

        [Required, Display(Name = "Finding Type")]
        public string FindingType { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public string Status { get; set; } = "Open";
        public DateTime? ResolvedDate { get; set; }
    }
}
