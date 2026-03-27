using Microsoft.EntityFrameworkCore;
using WelfareLink.Data;
using WelfareLink.Interfaces;
using WelfareLink.Models;

namespace WelfareLink.Repositories;

public class WelfareApplicationRepository : Repository<WelfareApplication> ,IWelfareApplicationRepository
{
    public WelfareApplicationRepository(WelfareLinkDbContext context) : base(context)
    {
    }
    public async Task<IEnumerable<WelfareApplication>> GetByStatusAsync(string status)
    {
        return await _dbSet
            .Where(a => a.Status == status)
            .ToListAsync();
    }

    public async Task<IEnumerable<WelfareApplication>> GetByCitizenIdAsync(int citizenId)
    {
        return await _dbSet
            .Where(a => a.CitizenID == citizenId)
            .OrderByDescending(a => a.SubmittedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<WelfareApplication>> GetByProgramIdAsync(int programId)
    {
        return await _dbSet
            .Where(a => a.ProgramID == programId)
            .OrderByDescending(a => a.SubmittedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<WelfareApplication>> GetPendingApplicationsAsync()
    {
        return await _dbSet
            .Where(a => a.Status == "Pending")
            .OrderBy(a => a.SubmittedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<WelfareApplication>> GetApplicationsByDateRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        return await _dbSet
            .Where(a => a.SubmittedDate >= startDate && a.SubmittedDate <= endDate)
            .OrderBy(a => a.SubmittedDate)
            .ToListAsync();
    }

    public async Task<bool> UpdateStatusAsync(int applicationId, string status)
    {
        var application = await GetByIdAsync(applicationId);
        if (application == null)
            return false;

        application.Status = status;
        await UpdateAsync(application);
        return true;
    }

}
