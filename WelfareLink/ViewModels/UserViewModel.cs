using System.ComponentModel.DataAnnotations;

namespace WelfareLink.ViewModels
{
    public class UserViewModel
    {
        public string UserID { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "Full Name")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Role is required")]
        [Display(Name = "User Role")]
        [StringLength(50)]
        public string Role { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email Address")]
        [StringLength(100)]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Phone Number")]
        [StringLength(20)]
        public string Phone { get; set; }

        // For display
        [Display(Name = "Audits Conducted")]
        public int AuditsCount { get; set; }
    }
}
