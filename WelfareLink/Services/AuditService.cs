using WelfareLink.Interfaces;
using WelfareLink.Repositories; 
using WelfareLink.Data;

using WelfareLink.Models;

namespace WelfareLink.Services;

public class AuditService : IAuditService
{
    private readonly IAuditRepository _repo;

    public AuditService(IAuditRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<Audit>> GetAllAuditsAsync()
    {
        return await _repo.GetAllAsync();
    }

    public async Task<Audit> GetAuditByIdAsync(string auditId)
    {
        return await _repo.GetByIdAsync(auditId);
    }

    public async Task<bool> CreateAuditAsync(Audit audit)
    {
        if (string.IsNullOrEmpty(audit.AuditID))
            audit.AuditID = Guid.NewGuid().ToString();

        audit.Date = DateTime.UtcNow;

        await _repo.AddAsync(audit);
        return true;
    }

    public async Task<bool> UpdateAuditStatusAsync(string auditId, string status, string findings)
    {
        var audit = await _repo.GetByIdAsync(auditId);
        if (audit == null)
            return false;

        audit.Status = status;
        audit.Findings = findings;

        await _repo.UpdateAsync(audit);
        return true;
    }
}
