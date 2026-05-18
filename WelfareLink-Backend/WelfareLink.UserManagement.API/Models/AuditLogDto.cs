namespace WelfareLink.UserManagement.API.Models
{
    public class AuditLogDto
    {
        public int LogID { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public int? EntityId { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? IPAddress { get; set; }
        public string? UserAgent { get; set; }
        public string Status { get; set; } = "Success";
        public DateTime Timestamp { get; set; }
    }
}
