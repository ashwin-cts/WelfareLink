using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WelfareLink.WApplicationSystem.API.Models
{
    public class Audit
    {
        [Key]
        public int AuditID { get; set; }

        public int? ProgramID { get; set; }

        public int AuditedByUserId { get; set; }

        public DateTime AuditDate { get; set; } = DateTime.UtcNow;

        [Required, StringLength(100)]
        public string FindingType { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Status { get; set; } = "Open";

        public DateTime? ResolvedDate { get; set; }

        [ForeignKey("ProgramID")]
        [JsonIgnore]
        public virtual WelfareProgram? WelfareProgram { get; set; }

        [ForeignKey("AuditedByUserId")]
        [JsonIgnore]
        public virtual User? AuditedByUser { get; set; }
    }
}
