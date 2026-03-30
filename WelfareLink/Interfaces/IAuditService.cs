using WelfareLink.Models;

namespace WelfareLink.Interfaces;

public interface IAuditService
<<<<<<< HEAD
{
    Task<IEnumerable<Audit>> GetAllAuditsAsync();
    Task<Audit> GetAuditByIdAsync(string auditId);
    Task<bool> CreateAuditAsync(Audit audit);
    Task<bool> UpdateAuditStatusAsync(string auditId, string status, string findings);
=======
{  
 
  Task<IEnumerable<Audit>> GetAllAuditsAsync();
 Task<Audit> GetAuditByIdAsync(string auditId);
 Task<bool> CreateAuditAsync(Audit audit);
 Task<bool> UpdateAuditStatusAsync(string auditId, string status, string findings);
>>>>>>> 67010b637ee5fae89ead73a246ac714beea4c426
}
