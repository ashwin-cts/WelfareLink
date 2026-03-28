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
            var benefits = await _context.Benefits
                .Include(b => b.Disbursements)
                .Include(b => b.WelfareApplication)
                    .ThenInclude(a => a.Program)
                .ToListAsync();

            foreach (var benefit in benefits)
            {
                if (benefit.WelfareApplication is not null)
                {
                    benefit.WelfareApplication.Citizen = new Citizen
                    {
                        CitizenID = benefit.WelfareApplication.CitizenID,
                        FullName = "citizenName"
                    };
                }
            }

            return benefits;
        }

        public async Task<Benefit?> GetByIdAsync(int id)
        {
            var benefit = await _context.Benefits
                .Include(b => b.Disbursements)
                .Include(b => b.WelfareApplication)
                    .ThenInclude(a => a.Program)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BenefitID == id);

            if (benefit?.WelfareApplication is not null)
            {
                benefit.WelfareApplication.Citizen = new Citizen
                {
                    CitizenID = benefit.WelfareApplication.CitizenID,
                    FullName = "citizenName"
                };
            }

            return benefit;
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

        public async Task<IEnumerable<Benefit>> GetByApplicationIdAsync(int applicationId)
        {
            return await _context.Benefits
                .Where(b => b.ApplicationID == applicationId)
                .ToListAsync();
        }
    }
}
