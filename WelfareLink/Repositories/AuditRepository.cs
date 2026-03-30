using Microsoft.EntityFrameworkCore;
using WelfareLink.Data; // Ensure this points to your DbContext folder
using WelfareLink.Interfaces;
<<<<<<< HEAD
using WelfareLink.Models;
using WelfareLink.Data;
using Microsoft.EntityFrameworkCore;
=======
using WelfareSystem.Models; // Ensure this points to your Models folder
>>>>>>> 67010b637ee5fae89ead73a246ac714beea4c426

namespace WelfareLink.Repositories
{
<<<<<<< HEAD
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
=======
    public class AuditRepository : IAuditRepository
    {
        private readonly ApplicationDbContext _context;

        public AuditRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: All audits ordered by date
        public async Task<IEnumerable<Audit>> GetAllAsync()
        {
            return await _context.Audits
                .Include(a => a.Officer) // Eagerly load Officer navigation data
                .OrderByDescending(a => a.Date)
                .ToListAsync();
        }

        // GET: Single audit by ID
        public async Task<Audit> GetByIdAsync(string auditId)
        {
            if (string.IsNullOrWhiteSpace(auditId)) return null;

            return await _context.Audits
                .Include(a => a.Officer)
                .FirstOrDefaultAsync(a => a.AuditID == auditId);
        }

        // CREATE: Add audit to DB context
        public async Task AddAsync(Audit audit)
        {
            if (audit == null) throw new ArgumentNullException(nameof(audit));

            await _context.Audits.AddAsync(audit);
            await _context.SaveChangesAsync();
        }

        // UPDATE: Save modifications to DB context
        public async Task UpdateAsync(Audit audit)
        {
            if (audit == null) throw new ArgumentNullException(nameof(audit));

            _context.Audits.Update(audit);
            await _context.SaveChangesAsync();
        }

        // DELETE: Wipe record from DB context
        public async Task DeleteAsync(string auditId)
        {
            if (string.IsNullOrWhiteSpace(auditId)) return;

            var audit = await _context.Audits.FindAsync(auditId);
            if (audit != null)
            {
                _context.Audits.Remove(audit);
                await _context.SaveChangesAsync();
            }
        }
>>>>>>> 67010b637ee5fae89ead73a246ac714beea4c426
    }
}

