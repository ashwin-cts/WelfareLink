using System.ComponentModel.DataAnnotations;

namespace WelfareLink.Models
{
    public class CitizenDocument
    {
        [Key]
        public int DocumentID { get; set; }

        [Required]
        public int CitizenID { get; set; }

        [Required]
        [StringLength(100)]
        public string DocumentType { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string DocumentPath { get; set; } = string.Empty;

        [Required]
        public DateTime UploadDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Active";
    }
}
