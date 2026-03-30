using WelfareLink.Models;

namespace WelfareLink.Interfaces;

public interface IComplianceRecordService
{
    Task<IEnumerable<ComplianceRecord>> GetAllRecordsAsync();
    Task<ComplianceRecord> GetRecordByIdAsync(string complianceId);
    Task<bool> CreateRecordAsync(ComplianceRecord record);
    Task<IEnumerable<ComplianceRecord>> GetRecordsByEntityAsync(string entityId);
}
