using Microsoft.EntityFrameworkCore;
using WelfareLink.Data; // Ensure this points to your DbContext folder
using WelfareLink.Interfaces;
using WelfareSystem.Models; // Ensure this points to your Models folder

namespace WelfareLink.Repositories
{
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
    }
}
