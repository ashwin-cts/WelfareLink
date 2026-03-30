using System.ComponentModel.DataAnnotations;

namespace WelfareLink.Models
{
    public class AuditLog
    {
        [Key]
        [Required]
        [StringLength(36)]
        public string LogID { get; set; }

        [Required]
        [StringLength(36)]
        public string UserID { get; set; }

        [Required]
        [StringLength(50)]
        public string Action { get; set; }

        [Required]
        [StringLength(100)]
        public string EntityType { get; set; }

        [StringLength(36)]
        public string EntityID { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        [StringLength(2000)]
        public string Details { get; set; }

        [StringLength(50)]
        public string IPAddress { get; set; }
    }
}
