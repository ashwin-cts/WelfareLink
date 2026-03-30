using WelfareLink.Models;

namespace WelfareLink.Interfaces;

public interface IAuditService
{
    Task<IEnumerable<Audit>> GetAllAuditsAsync();
    Task<Audit> GetAuditByIdAsync(string auditId);
    Task<bool> CreateAuditAsync(Audit audit);
    Task<bool> UpdateAuditStatusAsync(string auditId, string status, string findings);
}
