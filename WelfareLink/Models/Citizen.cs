using System.ComponentModel.DataAnnotations;
<<<<<<< HEAD
=======
using System.ComponentModel.DataAnnotations.Schema;
>>>>>>> origin/kamali

namespace WelfareLink.Models
{
    public class Citizen
    {
        [Key]
<<<<<<< HEAD
        public int CitizenID { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        public virtual ICollection<WelfareApplication>? WelfareApplications { get; set; }
=======
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int UserId { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
        [StringLength(300)]
        public string Address { get; set; }
        [StringLength(50)]
        public string ContactInfo { get; set; }
        [StringLength(50)]
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //Navigation Property one to many for CitizenDocument
        public virtual ICollection<CitizenDocument> CitizenDocuments { get; set; }



>>>>>>> origin/kamali
    }
}
