using WelfareLinkApi.Models;

namespace WelfareLinkApi.Interfaces;

public interface IAuditLogService
{
    Task<IEnumerable<AuditLog>> GetAllLogsAsync();
    Task<AuditLog?> GetLogByIdAsync(int id);
    Task<IEnumerable<AuditLog>> GetLogsByEntityAsync(string entityType, int entityId);
    Task<AuditLog> CreateLogAsync(int? userId, string action, string entityType, int entityId, string description);
}
