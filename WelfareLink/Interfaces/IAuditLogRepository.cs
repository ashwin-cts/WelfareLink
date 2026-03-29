using WelfareLink.Models;

namespace WelfareLink.Interfaces;

public interface IAuditLogRepository :IGenericRepository<AuditLog>
{
    
        Task<IEnumerable<AuditLog>> GetByUserIdAsync(string userId);
        Task<IEnumerable<AuditLog>> GetByActionAsync(string action);
        Task<IEnumerable<AuditLog>> GetByResourceAsync(string resource);
        Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<AuditLog>> GetFilteredLogsAsync(string? userId = null, string? action = null, string? resource = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<AuditLog>> GetRecentActivityAsync(int hours);
        Task<IEnumerable<AuditLog>> GetUserRecentActivityAsync(string userId, int hours);
        Task<int> GetActionCountAsync(string action, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<AuditLog>> GetSecurityEventsAsync();
        Task<IEnumerable<AuditLog>> GetLoginAttemptsAsync(DateTime startDate, DateTime endDate);
    }

