using WelfareLink.Models;

namespace WelfareLink.Interfaces;

public interface IAuditLogService
{
    Task LogActionAsync(string userId, string action, string resource, string? details = null, string? ipAddress = null);
    Task<IEnumerable<AuditLog>> GetAuditLogsAsync(AuditLogFilter filter);
    Task<IEnumerable<AuditLog>> GetUserActivityAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<AuditLog>> GetRecentActivityAsync(int hours = 24);
    Task<IEnumerable<AuditLog>> GetSecurityEventsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<AuditLog>> GetLoginAttemptsAsync(DateTime startDate, DateTime endDate);
    Task<int> GetActionCountAsync(string action, DateTime? startDate = null, DateTime? endDate = null);
    Task<Dictionary<string, int>> GetActivityStatisticsAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<AuditLog>> GetFailedLoginAttemptsAsync(string? userId = null, DateTime? startDate = null);
    Task<bool> CleanupOldAuditLogsAsync(int daysToKeep);
}

public class AuditLogFilter
{
    public string? UserId { get; set; }
    public string? Action { get; set; }
    public string? Resource { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

