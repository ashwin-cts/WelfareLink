using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WelfareLink.Data;
using WelfareLink.Interfaces;
using WelfareLink.Models;

namespace WelfareLink.Repositories;

public class AuditRepository : IAuditRepository
{
    private readonly WelfareLinkDbContext _db;

    public AuditRepository(WelfareLinkDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Audit>> GetAllAsync()
    {
        return await _db.Audits.ToListAsync();
    }

    public async Task<Audit?> GetByIdAsync(string id)
    {
        return await _db.Audits.FindAsync(id);
    }

    public async Task AddAsync(Audit audit)
    {
        await _db.Audits.AddAsync(audit);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Audit audit)
    {
        _db.Audits.Update(audit);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id)
    {
        var audit = await GetByIdAsync(id);
        if (audit is not null)
        {
            _db.Audits.Remove(audit);
            await _db.SaveChangesAsync();
        }
    }
}
