using WelfareLink.Models;

namespace WelfareLink.Interfaces;

public interface IAuditRepository
{
    Task<IEnumerable<Audit>> GetAllAsync();
    Task<Audit> GetByIdAsync(string auditId);
    Task AddAsync(Audit audit);
    Task UpdateAsync(Audit audit);
    Task DeleteAsync(string auditId);

}
