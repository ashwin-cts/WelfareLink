using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WelfareLink.Models
{
    public class Resource
    {
        [Key]
        [Required]
        [StringLength(36)]
        public string ResourceID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(50)]
        public string Type { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalBudget { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AllocatedBudget { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RemainingBudget { get; set; }

        [StringLength(30)]
        public string Status { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
