using WelfareLinkApi.Models;

namespace WelfareLinkApi.Interfaces;

public interface IAuditLogRepository
{
    Task<IEnumerable<AuditLog>> GetAllAsync();
    Task<AuditLog?> GetByIdAsync(int id);
    Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, int entityId);
    Task<IEnumerable<AuditLog>> GetByUserIdAsync(int userId);
    Task<AuditLog> AddAsync(AuditLog log);
}
