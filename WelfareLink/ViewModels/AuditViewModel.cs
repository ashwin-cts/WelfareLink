using System.ComponentModel.DataAnnotations;
using WelfareLink.Models;

namespace WelfareLink.ViewModels
{
    public class AuditViewModel
    {
        public string AuditID { get; set; }

        [Required(ErrorMessage = "Officer is required")]
        [Display(Name = "Audit Officer")]
        public string OfficerID { get; set; }

        [Required(ErrorMessage = "Scope is required")]
        [StringLength(500, ErrorMessage = "Scope cannot exceed 500 characters")]
        [Display(Name = "Audit Scope")]
        public string Scope { get; set; }

        [StringLength(4000, ErrorMessage = "Findings cannot exceed 4000 characters")]
        [Display(Name = "Audit Findings")]
        public string Findings { get; set; }

        [Required(ErrorMessage = "Date is required")]
        [Display(Name = "Audit Date")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [StringLength(30)]
        [Display(Name = "Audit Status")]
        public string Status { get; set; }

        // For display purposes
        [Display(Name = "Officer Name")]
        public string OfficerName { get; set; }

        // List of officers for dropdown
        public List<User> AvailableOfficers { get; set; } = new List<User>();
    }
}
