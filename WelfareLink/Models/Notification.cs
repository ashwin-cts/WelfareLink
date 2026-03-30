using System.ComponentModel.DataAnnotations;

namespace WelfareLink.Models
{
    public class Notification
    {
        [Key]
        [Required]
        [StringLength(36)]
        public string NotificationID { get; set; }

        [Required]
        [StringLength(36)]
        public string RecipientID { get; set; }

        [Required]
        [StringLength(50)]
        public string Type { get; set; }

        [Required]
        [StringLength(200)]
        public string Subject { get; set; }

        [Required]
        [StringLength(2000)]
        public string Message { get; set; }

        [Required]
        public DateTime SentDate { get; set; }

        [Required]
        public bool IsRead { get; set; }

        [StringLength(30)]
        public string Priority { get; set; }
    }
}
