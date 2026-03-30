using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WelfareLink.Models
{
    public class WelfareProgram
    {
        [Key]
        [Required]
        [StringLength(36)]
        public string ProgramID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [StringLength(50)]
        public string Category { get; set; }

        [StringLength(2000)]
        public string EligibilityCriteria { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BudgetAllocated { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [StringLength(30)]
        public string Status { get; set; }

        // Navigation properties
        public virtual ICollection<WelfareApplication> Applications { get; set; }
        public virtual ICollection<Benefit> Benefits { get; set; }
    }
}
