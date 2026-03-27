using System.ComponentModel.DataAnnotations;

namespace WelfareLink.Models
{
    public class WelfareApplication
    {
        [Key]
        public int ApplicationID { get; set; }

        [Required(ErrorMessage = "Citizen ID is required")]
        [Display(Name = "Citizen ID")]
        public int CitizenID { get; set; }

        [Required(ErrorMessage = "Program ID is required")]
        [Display(Name = "Program ID")]
        public int ProgramID { get; set; }

        [Required]
        [Display(Name = "Submitted Date")]
        public DateOnly SubmittedDate { get; set; }

        [Required]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending";
    }
}
