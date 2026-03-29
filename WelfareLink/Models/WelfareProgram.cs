using System.ComponentModel.DataAnnotations;

namespace WelfareLink.Models
{
    public class WelfareProgram
    {
        [Key]
        public int ProgramID { get; set; }

        [Required]
        [StringLength(200)]
        public string ProgramName { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public double MaxBenefit { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Active";

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
