using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WelfareLink.Models
{
    public class CitizenDocument
    {
        [Key]
        [Required]
        [StringLength(36)]
        public string DocumentID { get; set; }

        [Required]
        [StringLength(36)]
        [ForeignKey("Citizen")]
        public string CitizenID { get; set; }

        [Required]
        [StringLength(50)]
        public string DocumentType { get; set; }

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; }

        [Required]
        public DateTime UploadDate { get; set; }

        [StringLength(30)]
        public string Status { get; set; }

        // Navigation property
        public virtual Citizen Citizen { get; set; }
    }
}
