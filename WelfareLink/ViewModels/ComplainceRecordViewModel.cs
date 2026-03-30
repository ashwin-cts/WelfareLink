using System.ComponentModel.DataAnnotations;

namespace WelfareLink.ViewModels
{
    public class ComplainceRecordViewModel
    {
        public string ComplianceID { get; set; }

        [Required(ErrorMessage = "Entity ID is required")]
        [Display(Name = "Entity ID")]
        [StringLength(36)]
        public string EntityID { get; set; }

        [Required(ErrorMessage = "Type is required")]
        [Display(Name = "Compliance Type")]
        [StringLength(30)]
        public string Type { get; set; }

        [Required(ErrorMessage = "Result is required")]
        [Display(Name = "Compliance Result")]
        [StringLength(30)]
        public string Result { get; set; }

        [Required(ErrorMessage = "Date is required")]
        [Display(Name = "Compliance Date")]
        public DateTime Date { get; set; }

        [StringLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters")]
        [Display(Name = "Notes")]
        public string Notes { get; set; }

        // For display
        public string EntityName { get; set; }
    }
}
