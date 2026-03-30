using WelfareLink.Models;

namespace WelfareLink.Interfaces;

public interface IComplianceRecordRepository
{
    Task<IEnumerable<ComplianceRecord>> GetAllRecordsAsync();
    Task<ComplianceRecord> GetRecordByIdAsync(string complianceId);
    Task<bool> CreateRecordAsync(ComplianceRecord record);
    Task<IEnumerable<ComplianceRecord>> GetRecordsByEntityAsync(string entityId);
}
