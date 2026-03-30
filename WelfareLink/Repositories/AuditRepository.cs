using WelfareLink.Interfaces;
using WelfareLink.Models;
using WelfareLink.Data;
using Microsoft.EntityFrameworkCore;

namespace WelfareLink.Repositories;

public class AuditRepository : IAuditRepository
{
    private readonly WelfareLinkDbContext _context;

    public AuditRepository(WelfareLinkDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Audit>> GetAllAsync()
    {
        return await _context.Audits
            .Include(a => a.Officer)
            .OrderByDescending(a => a.Date)
            .ToListAsync();
    }

    public async Task<Audit> GetByIdAsync(string auditId)
    {
        return await _context.Audits
            .Include(a => a.Officer)
            .FirstOrDefaultAsync(a => a.AuditID == auditId);
    }

    public async Task AddAsync(Audit audit)
    {
        await _context.Audits.AddAsync(audit);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Audit audit)
    {
        _context.Audits.Update(audit);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string auditId)
    {
        var audit = await _context.Audits.FindAsync(auditId);
        if (audit != null)
        {
            _context.Audits.Remove(audit);
            await _context.SaveChangesAsync();
        }
    }
}

