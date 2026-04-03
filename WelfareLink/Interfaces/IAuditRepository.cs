using System.Collections.Generic;
using System.Threading.Tasks;
using WelfareLink.Models;

namespace WelfareLink.Interfaces;

public interface IAuditRepository
{
    Task<IEnumerable<Audit>> GetAllAsync();

    Task<Audit?> GetByIdAsync(string id);

    Task AddAsync(Audit audit);

    Task UpdateAsync(Audit audit);

    Task DeleteAsync(string id);
}
