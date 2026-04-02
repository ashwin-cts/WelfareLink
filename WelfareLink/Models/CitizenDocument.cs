using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WelfareLink.Models
{
    public class CitizenDocument
    {
        [Key]
        public int DocumentID { get; set; }

        [Required]
        [ForeignKey("Citizen")]
        public int CitizenId { get; set; } // Updated to match the Citizen model

        [StringLength(30)]
        public string DocType { get; set; } // e.g., "IDProof", "Residence"

        [StringLength(100)]
        [Display(Name = "Document Name")]
        public string? DocumentName { get; set; }

        [StringLength(500)]
        public string FileURI { get; set; }

        public DateTime UploadedDate { get; set; } = DateTime.UtcNow;

        [StringLength(30)]
        public string VerificationStatus { get; set; }

        // Navigation property mapping back to the Citizen
        [JsonIgnore]
        public virtual Citizen Citizen { get; set; }




    }
}
