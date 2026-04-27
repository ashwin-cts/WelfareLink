using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WelfareLink.AnalyticsReport.API.Models
{
    public class AuditLog
    {
        [Key]
        public int LogID { get; set; }

        public int? UserId { get; set; }

        [Required, StringLength(100)]
        public string Action { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string EntityType { get; set; } = string.Empty;

        public int? EntityId { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        // Store old and new values for comparison
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }

        // IP Address and User Agent for security
        [StringLength(45)] // IPv6 max length
        public string? IPAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        // Status outcome
        [StringLength(50)]
        public string Status { get; set; } = "Success";

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        [JsonIgnore]
        public virtual User? User { get; set; }
    }
}
