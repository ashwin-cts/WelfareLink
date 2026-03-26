using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace WelfareLink.Models
{
    public class EligibilityCheck
    {
        [Key]
        public int CheckID { get; set; }

        [Required(ErrorMessage = "Application ID is required")]
        [Display(Name = "Application ID")]
        public int ApplicationID { get; set; }

        [Required(ErrorMessage = "Officer ID is required")]
        [Display(Name = "Officer ID")]
        public int OfficerID { get; set; }

        [Required]
        [Display(Name = "Result")]
        public string Result { get; set; }

        [Required]
        [Display(Name = "Result Code")]
        public string ResultCode { get; set; }

        [Required]
        [Display(Name = "Assessment Date")]
        public DateOnly Date { get; set; }

        [Required]
        [Display(Name = "Notes")]
        public string Notes { get; set; }

        // Navigation properties (optional - for EF Core relationships)
        [ForeignKey("ApplicationID")]
        public WelfareApplication? WelfareApplication { get; set; }
    }
}

