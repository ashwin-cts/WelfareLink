using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WelfareLinkApi.Models
{
    public class ComplainceRecord
    {
        [Key]
        public int RecordID { get; set; }

        public int? RaisedByUserId { get; set; }

        [Required, StringLength(100)]
        public string EntityType { get; set; } = string.Empty;

        public int EntityId { get; set; }

        // Specific entity details for tracking
        public int? BenefitID { get; set; }
        public int? DisbursementID { get; set; }
        public int? ApplicationID { get; set; }
        public int? CitizenID { get; set; }

        [Required, StringLength(100)]
        public string ViolationType { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Status { get; set; } = "Open";

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ResolvedDate { get; set; }

        public int? ResolvedByUserId { get; set; }

        public string? Notes { get; set; }

        [ForeignKey("RaisedByUserId")]
        [JsonIgnore]
        public virtual User? RaisedByUser { get; set; }

        [ForeignKey("ResolvedByUserId")]
        [JsonIgnore]
        public virtual User? ResolvedByUser { get; set; }

        // Expose the user names for API responses (not mapped to DB)
        [NotMapped]
        public string? RaisedByUserName => RaisedByUser?.FullName ?? RaisedByUser?.Username;

        [NotMapped]
        public string? ResolvedByUserName => ResolvedByUser?.FullName ?? ResolvedByUser?.Username;

        // Optional contextual fields, populated by repository/service when available
        [NotMapped]
        public string? CitizenName { get; set; }

        [NotMapped]
        public string? ProgramTitle { get; set; }
    }
}
