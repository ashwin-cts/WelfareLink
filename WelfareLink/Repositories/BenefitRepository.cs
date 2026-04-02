using Microsoft.EntityFrameworkCore;
using WelfareLink.Data;
using WelfareLink.Interfaces;
using WelfareLink.Models;

namespace WelfareLink.Repositories
{
    public class BenefitRepository : IBenefitRepository
    {
        private readonly WelfareLinkDbContext _context;

        public BenefitRepository(WelfareLinkDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Benefit>> GetAllAsync()
        {
            // Return AsNoTracking to avoid tracking conflicts when callers also
            // pass detached/new Benefit instances to update methods.
            return await _context.Benefits
                .AsNoTracking()
                .Include(b => b.Disbursements)
                .Include(b => b.WelfareApplication)
                    .ThenInclude(a => a.Program)
                .Include(b => b.WelfareApplication)
                    .ThenInclude(a => a.Citizen)
                .ToListAsync();
        }

        public async Task<Benefit?> GetByIdAsync(int id)
        {
            return await _context.Benefits
                .Include(b => b.Disbursements)
                .Include(b => b.WelfareApplication)
                    .ThenInclude(a => a.Program)
                .Include(b => b.WelfareApplication)
                    .ThenInclude(a => a.Citizen)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BenefitID == id);
        }

        public async Task<Benefit> AddAsync(Benefit benefit)
        {
            _context.Benefits.Add(benefit);
            await _context.SaveChangesAsync();
            return benefit;
        }

        public async Task<Benefit> UpdateAsync(Benefit benefit)
        {
            // If a Benefit with the same key is already tracked, detach it first
            // to avoid the "instance cannot be tracked" exception.
            var tracked = _context.ChangeTracker.Entries<Benefit>()
                .FirstOrDefault(e => e.Entity.BenefitID == benefit.BenefitID);
            if (tracked != null)
            {
                _context.Entry(tracked.Entity).State = EntityState.Detached;
            }

            _context.Entry(benefit).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return benefit;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var benefit = await _context.Benefits.FindAsync(id);
            if (benefit == null)
            {
                return false;
            }

            _context.Benefits.Remove(benefit);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Benefits.AnyAsync(e => e.BenefitID == id);
        }

        public async Task<IEnumerable<Benefit>> GetByApplicationIdAsync(int applicationId)
        {
            // Use AsNoTracking so returned entities do not remain tracked.
            return await _context.Benefits
                .AsNoTracking()
                .Where(b => b.ApplicationID == applicationId)
                .ToListAsync();
        }
    }
}
