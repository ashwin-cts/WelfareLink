using WelfareLink.ComplianceAndAudit.API.Models;

namespace WelfareLink.ComplianceAndAudit.API.Interfaces
{
    public interface IAuditLogService
    {
        Task<IEnumerable<AuditLog>> GetAllAuditLogsAsync();
        Task<AuditLog?> GetAuditLogByIdAsync(int id);
        Task<(IEnumerable<AuditLog> logs, int totalCount)> GetPagedAuditLogsAsync(int pageNumber, int pageSize);
        Task<(IEnumerable<AuditLog> logs, int totalCount)> GetPagedAuditLogsByEntityTypeAsync(string entityType, int pageNumber, int pageSize);
        Task<(IEnumerable<AuditLog> logs, int totalCount)> GetPagedAuditLogsByActionAsync(string action, int pageNumber, int pageSize);
        Task<(IEnumerable<AuditLog> logs, int totalCount)> GetPagedAuditLogsByDateRangeAsync(DateTime startDate, DateTime endDate, int pageNumber, int pageSize);
        Task<AuditLog> LogActionAsync(int? userId, string action, string entityType, int? entityId, string description, string? oldValue = null, string? newValue = null, string? ipAddress = null, string? userAgent = null, string status = "Success");
        Task<bool> DeleteAuditLogAsync(int id);
        Task<bool> DeleteOldAuditLogsAsync(int daysOld);
    }
}
