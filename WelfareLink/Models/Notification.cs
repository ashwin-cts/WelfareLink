using System.ComponentModel.DataAnnotations;

namespace WelfareLink.Models
{
    public class Notification
    {
        [Key]
        public string NotificationID { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string UserID { get; set; } = string.Empty;

        [StringLength(100)]
        public string EntityID { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Message { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Unread";

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
    

