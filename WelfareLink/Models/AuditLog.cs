using System.ComponentModel.DataAnnotations;

namespace WelfareLink.Models
{
    public class AuditLog
    {
        [Key]
        public string AuditLogID { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string UserID { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Action { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Resource { get; set; } = string.Empty;

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
