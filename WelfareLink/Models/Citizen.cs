using System.ComponentModel.DataAnnotations;

namespace WelfareLink.Models
{
    public class Citizen
    {
        [Key]
        public int CitizenID { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        public virtual ICollection<WelfareApplication>? WelfareApplications { get; set; }
    }
}
