using System.Collections.Generic;
using System.Threading.Tasks;
using WelfareLink.Models;

namespace WelfareLink.Interfaces;

public interface IAuditService
{
    Task<IEnumerable<Audit>> GetAllAsync();

    Task<Audit?> GetByIdAsync(string id);

    Task CreateAsync(Audit audit);

    Task UpdateAsync(Audit audit);

    Task DeleteAsync(string id);
}
