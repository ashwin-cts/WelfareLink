using Microsoft.EntityFrameworkCore;
using WelfareLink.Interfaces;
using WelfareLink.Models;
using WelfareLink.Data;

namespace WelfareLink.Repositories;

public class EligibilityCheckRepository : Repository<EligibilityCheck>, IEligibilityCheckRepository
{
    public EligibilityCheckRepository(WelfareLinkDbContext context) : base(context)
    {
    }
    public async Task<IEnumerable<EligibilityCheck>> GetByApplicationIdAsync(int applicationId)
    {
        return await _dbSet
            .Include(e => e.WelfareApplication)
            .Where(e => e.ApplicationID == applicationId)
            .OrderByDescending(e => e.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<EligibilityCheck>> GetByOfficerIdAsync(int officerId)
    {
        return await _dbSet
            .Include(e => e.WelfareApplication)
            .Where(e => e.OfficerID == officerId)
            .OrderByDescending(e => e.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<EligibilityCheck>> GetByResultAsync(string result)
    {
        return await _dbSet
            .Include(e => e.WelfareApplication)
            .Where(e => e.Result == result)
            .OrderByDescending(e => e.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<EligibilityCheck>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        return await _dbSet
            .Include(e => e.WelfareApplication)
            .Where(e => e.Date >= startDate && e.Date <= endDate)
            .OrderBy(e => e.Date)
            .ToListAsync();
    }

    public async Task<EligibilityCheck?> GetLatestCheckForApplicationAsync(int applicationId)
    {
        return await _dbSet
            .Include(e => e.WelfareApplication)
            .Where(e => e.ApplicationID == applicationId)
            .OrderByDescending(e => e.Date)
            .FirstOrDefaultAsync();
    }

    public override async Task<IEnumerable<EligibilityCheck>> GetAllAsync()
    {
        return await _dbSet
            .Include(e => e.WelfareApplication)
            .ToListAsync();
    }

    public override async Task<EligibilityCheck?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(e => e.WelfareApplication)
            .FirstOrDefaultAsync(e => e.CheckID == id);
    }
}
