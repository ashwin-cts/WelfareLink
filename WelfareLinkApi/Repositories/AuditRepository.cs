using Microsoft.EntityFrameworkCore;
using WelfareLinkApi.Data;
using WelfareLinkApi.Interfaces;
using WelfareLinkApi.Models;

namespace WelfareLinkApi.Repositories;

public class AuditRepository : IAuditRepository
{
    private readonly WelfareLinkDbContext _context;

    public AuditRepository(WelfareLinkDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Audit>> GetAllAsync()
        => await _context.Audits
            .Include(a => a.WelfareProgram)
            .Include(a => a.AuditedByUser)
            .OrderByDescending(a => a.AuditDate)
            .ToListAsync();

    public async Task<Audit?> GetByIdAsync(int id)
        => await _context.Audits
            .Include(a => a.WelfareProgram)
            .Include(a => a.AuditedByUser)
            .FirstOrDefaultAsync(a => a.AuditID == id);

    public async Task<IEnumerable<Audit>> GetByProgramIdAsync(int programId)
        => await _context.Audits
            .Where(a => a.ProgramID == programId)
            .OrderByDescending(a => a.AuditDate)
            .ToListAsync();

    public async Task<Audit> AddAsync(Audit audit)
    {
        await _context.Audits.AddAsync(audit);
        await _context.SaveChangesAsync();
        return audit;
    }

    public async Task UpdateAsync(Audit audit)
    {
        _context.Entry(audit).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }
}
