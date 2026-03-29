using System.ComponentModel.DataAnnotations;

namespace WelfareLink.Models
{
    public class Resource
    {
        [Key]
        public int ResourceID { get; set; }

        [Required]
        [StringLength(200)]
        public string ResourceName { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int AvailableQuantity { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Available";
    }
}
