<<<<<<< HEAD
using WelfareLink.Models;

namespace WelfareLink.Interfaces;
=======
>>>>>>> 67010b637ee5fae89ead73a246ac714beea4c426

using WelfareLink.Models;

namespace WelfareLink.Interfaces;
public interface IAuditRepository
{
<<<<<<< HEAD
    Task<IEnumerable<Audit>> GetAllAsync();
    Task<Audit> GetByIdAsync(string auditId);
    Task AddAsync(Audit audit);
    Task UpdateAsync(Audit audit);
    Task DeleteAsync(string auditId);

=======
   Task<IEnumerable<Audit>> GetAllAsync();
 Task<Audit> GetByIdAsync(string auditId);
 Task AddAsync(Audit audit);
 Task UpdateAsync(Audit audit);
 Task DeleteAsync(string auditId);
>>>>>>> 67010b637ee5fae89ead73a246ac714beea4c426
}
