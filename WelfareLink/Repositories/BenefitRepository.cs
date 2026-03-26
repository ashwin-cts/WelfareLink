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
            return await _context.Benefits
                .Include(b => b.Disbursements)
                .ToListAsync();
        }

        public async Task<Benefit?> GetByIdAsync(int id)
        {
            return await _context.Benefits
                .Include(b => b.Disbursements)
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
    }
}
