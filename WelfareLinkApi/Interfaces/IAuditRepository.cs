using WelfareLinkApi.Models;

namespace WelfareLinkApi.Interfaces;

public interface IAuditRepository
{
    Task<IEnumerable<Audit>> GetAllAsync();
    Task<Audit?> GetByIdAsync(int id);
    Task<IEnumerable<Audit>> GetByProgramIdAsync(int programId);
    Task<Audit> AddAsync(Audit audit);
    Task UpdateAsync(Audit audit);
}
