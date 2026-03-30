using System.ComponentModel.DataAnnotations;

namespace WelfareLink.Models
{
    public class User
    {
        [Key]
        [Required]
        [StringLength(36)]
        public string UserID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(50)]
        public string Role { get; set; }

        [Required]
        [StringLength(100)]
        public string Email { get; set; }

        [StringLength(20)]
        public string Phone { get; set; }

        // Navigation property for audits conducted by this officer
        public virtual ICollection<Audit> ConductedAudits { get; set; }
    }
}
