using System.Collections.Generic;
using System.Threading.Tasks;
using WelfareLink.Interfaces;
using WelfareLink.Models;

namespace WelfareLink.Services;

public class AuditService : IAuditService
{
    private readonly IAuditRepository _repo;

    public AuditService(IAuditRepository repo)
    {
        _repo = repo;
    }

    public Task<IEnumerable<Audit>> GetAllAsync() => _repo.GetAllAsync();

    public Task<Audit?> GetByIdAsync(string id) => _repo.GetByIdAsync(id);

    public Task CreateAsync(Audit audit) => _repo.AddAsync(audit);

    public Task UpdateAsync(Audit audit) => _repo.UpdateAsync(audit);

    public Task DeleteAsync(string id) => _repo.DeleteAsync(id);
}
