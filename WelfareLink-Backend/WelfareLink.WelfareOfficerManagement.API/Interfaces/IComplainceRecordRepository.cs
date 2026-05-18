using WelfareLink.WelfareOfficerManagement.API.Models;

namespace WelfareLink.WelfareOfficerManagement.API.Interfaces;

public interface IComplainceRecordRepository
{
    Task<IEnumerable<ComplainceRecord>> GetAllAsync();
    Task<ComplainceRecord?> GetByIdAsync(int id);
    Task<IEnumerable<ComplainceRecord>> GetByStatusAsync(string status);
    Task<IEnumerable<ComplainceRecord>> GetByEntityAsync(string entityType, int entityId);
    Task<ComplainceRecord> AddAsync(ComplainceRecord record);
    Task UpdateAsync(ComplainceRecord record);
}
