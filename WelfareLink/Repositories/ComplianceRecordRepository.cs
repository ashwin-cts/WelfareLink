using WelfareLink.Interfaces;
using WelfareLink.Models;
using WelfareLink.Data;
using Microsoft.EntityFrameworkCore;

namespace WelfareLink.Repositories;

public class ComplianceRecordRepository : IComplianceRecordRepository
{
    private readonly WelfareLinkDbContext _context;

    public ComplianceRecordRepository(WelfareLinkDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ComplianceRecord>> GetAllRecordsAsync()
    {
        return await _context.ComplianceRecords
            .OrderByDescending(c => c.Date)
            .ToListAsync();
    }

    public async Task<ComplianceRecord> GetRecordByIdAsync(string complianceId)
    {
        return await _context.ComplianceRecords.FindAsync(complianceId);
    }

    public async Task<bool> CreateRecordAsync(ComplianceRecord record)
    {
        await _context.ComplianceRecords.AddAsync(record);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<ComplianceRecord>> GetRecordsByEntityAsync(string entityId)
    {
        return await _context.ComplianceRecords
            .Where(r => r.EntityID == entityId)
            .OrderByDescending(c => c.Date)
            .ToListAsync();
    }
}
