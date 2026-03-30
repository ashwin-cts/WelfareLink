using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WelfareLink.Models
{
    public class Citizen
    {
        [Key]
        [Required]
        [StringLength(36)]
        public string CitizenID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(20)]
        public string NationalID { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [StringLength(10)]
        public string Gender { get; set; }

        [StringLength(200)]
        public string Address { get; set; }

        [StringLength(20)]
        public string Phone { get; set; }

        [StringLength(100)]
        public string Email { get; set; }

        [StringLength(50)]
        public string EmploymentStatus { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyIncome { get; set; }

        public int HouseholdSize { get; set; }

        // Navigation properties
        public virtual ICollection<CitizenDocument> Documents { get; set; }
        public virtual ICollection<WelfareApplication> Applications { get; set; }
    }
}
