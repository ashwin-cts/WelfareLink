using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WelfareLink.Models
{
    public class WelfareApplication
    {
        [Key]
        [Required]
        [StringLength(36)]
        public string ApplicationID { get; set; }

        [Required]
        [StringLength(36)]
        [ForeignKey("Citizen")]
        public string CitizenID { get; set; }

        [Required]
        [StringLength(36)]
        [ForeignKey("WelfareProgram")]
        public string ProgramID { get; set; }

        [Required]
        public DateTime ApplicationDate { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; }

        public DateTime? ReviewDate { get; set; }

        [StringLength(36)]
        public string ReviewedBy { get; set; }

        [StringLength(2000)]
        public string Comments { get; set; }

        // Navigation properties
        public virtual Citizen Citizen { get; set; }
        public virtual WelfareProgram WelfareProgram { get; set; }
        public virtual ICollection<EligibilityCheck> EligibilityChecks { get; set; }
        public virtual ICollection<Benefit> Benefits { get; set; }
    }
}
