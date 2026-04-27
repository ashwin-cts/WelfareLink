using WelfareLink.Operations.API.Models;

namespace WelfareLink.Operations.API.Interfaces
{
    public interface IAuditLogRepository
    {
        Task<IEnumerable<AuditLog>> GetAllAsync();
        Task<AuditLog?> GetByIdAsync(int id);
        Task<(IEnumerable<AuditLog> logs, int totalCount)> GetPagedAsync(int pageNumber, int pageSize);
        Task<(IEnumerable<AuditLog> logs, int totalCount)> GetPagedByEntityTypeAsync(string entityType, int pageNumber, int pageSize);
        Task<(IEnumerable<AuditLog> logs, int totalCount)> GetPagedByActionAsync(string action, int pageNumber, int pageSize);
        Task<(IEnumerable<AuditLog> logs, int totalCount)> GetPagedByDateRangeAsync(DateTime startDate, DateTime endDate, int pageNumber, int pageSize);
        Task<AuditLog> AddAsync(AuditLog auditLog);
        Task<bool> DeleteAsync(int id);
        Task<bool> DeleteOldLogsAsync(int daysOld);
    }
}
