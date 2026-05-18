using WelfareLink.ProgramManagement.API.Models;

namespace WelfareLink.ProgramManagement.API.Interfaces;

public interface IComplainceRecordService
{
    Task<IEnumerable<ComplainceRecord>> GetAllRecordsAsync();
    Task<ComplainceRecord?> GetRecordByIdAsync(int id);
    Task<IEnumerable<ComplainceRecord>> GetOpenRecordsAsync();
    Task<ComplainceRecord> CreateRecordAsync(ComplainceRecord record);
    Task<ComplainceRecord> UpdateStatusAsync(int id, string status, int? resolvedByUserId, string? notes);
}
