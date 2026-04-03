using System;
using System.ComponentModel.DataAnnotations;

namespace WelfareLink.ViewModels
{
    public class AuditViewModel
    {
        [Display(Name = "Audit ID")]
        public string AuditID { get; set; } = string.Empty;

        [Display(Name = "Officer ID")]
        public string OfficerID { get; set; } = string.Empty;

        [Display(Name = "Scope")]
        public string Scope { get; set; } = string.Empty;

        [Display(Name = "Findings")]
        public string Findings { get; set; } = string.Empty;

        [Display(Name = "Date")]
        public DateTime Date { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } = string.Empty;
    }

    public class AuditCreateModel
    {
        [Required]
        [MaxLength(36)]
        [Display(Name = "Officer ID")]
        public string OfficerID { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Scope { get; set; } = string.Empty;

        [MaxLength(4000)]
        public string Findings { get; set; } = string.Empty;

        [MaxLength(30)]
        public string Status { get; set; } = string.Empty;
    }
}
