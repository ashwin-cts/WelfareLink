using Microsoft.EntityFrameworkCore;
using WelfareLink.UserManagement.API.Data;
using WelfareLink.UserManagement.API.Interfaces;
using WelfareLink.UserManagement.API.Models;

namespace WelfareLink.UserManagement.API.Repositories;

public class ComplainceRecordRepository : IComplainceRecordRepository
{
    private readonly WelfareLinkDbContext _context;

    public ComplainceRecordRepository(WelfareLinkDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ComplainceRecord>> GetAllAsync()
        => await _context.ComplianceRecords
            .Include(r => r.RaisedByUser)
            .Include(r => r.ResolvedByUser)
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync();

    public async Task<ComplainceRecord?> GetByIdAsync(int id)
        {
            var rec = await _context.ComplianceRecords
                .Include(r => r.RaisedByUser)
                .Include(r => r.ResolvedByUser)
                .FirstOrDefaultAsync(r => r.RecordID == id);

            if (rec != null)
            {
                // Populate some contextual data if the record references other entities
                if (rec.CitizenID.HasValue)
                {
                    var citizen = await _context.Citizens.FindAsync(rec.CitizenID.Value);
                    if (citizen != null) rec.CitizenName = citizen.Name;
                }

                if (rec.ApplicationID.HasValue)
                {
                    var app = await _context.WelfareApplications.FindAsync(rec.ApplicationID.Value);
                    if (app != null)
                    {
                        var program = await _context.Programs.FindAsync(app.ProgramID);
                        if (program != null) rec.ProgramTitle = program.Title;
                    }
                }
            }

            return rec;
        }

    public async Task<IEnumerable<ComplainceRecord>> GetByStatusAsync(string status)
        => await _context.ComplianceRecords
            .Include(r => r.RaisedByUser)
            .Where(r => r.Status == status)
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync();

    public async Task<IEnumerable<ComplainceRecord>> GetByEntityAsync(string entityType, int entityId)
        => await _context.ComplianceRecords
            .Where(r => r.EntityType == entityType && r.EntityId == entityId)
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync();

    public async Task<ComplainceRecord> AddAsync(ComplainceRecord record)
    {
        await _context.ComplianceRecords.AddAsync(record);
        await _context.SaveChangesAsync();
        return record;
    }

    public async Task UpdateAsync(ComplainceRecord record)
    {
        _context.Entry(record).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }
}
