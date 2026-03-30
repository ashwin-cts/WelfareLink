using WelfareLink.Interfaces;
using WelfareLink.Models;
using WelfareLink.Data;
using Microsoft.EntityFrameworkCore;

namespace WelfareLink.Services;

public class ComplianceRecordService : IComplianceRecordService
{
    private readonly IComplianceRecordRepository _repo;

    public ComplianceRecordService(IComplianceRecordRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<ComplianceRecord>> GetAllRecordsAsync()
    {
        return await _repo.GetAllRecordsAsync();
    }

    public async Task<ComplianceRecord> GetRecordByIdAsync(string complianceId)
    {
        return await _repo.GetRecordByIdAsync(complianceId);
    }

    public async Task<bool> CreateRecordAsync(ComplianceRecord record)
    {
        if (string.IsNullOrEmpty(record.ComplianceID))
            record.ComplianceID = Guid.NewGuid().ToString();

        record.Date = DateTime.UtcNow;

        await _repo.CreateRecordAsync(record);
        return true;
    }

    public async Task<IEnumerable<ComplianceRecord>> GetRecordsByEntityAsync(string entityId)
    {
        return await _repo.GetRecordsByEntityAsync(entityId);
    }
}
