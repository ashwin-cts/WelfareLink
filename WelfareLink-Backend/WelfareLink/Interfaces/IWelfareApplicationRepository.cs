using WelfareLink.Models;

namespace WelfareLink.Interfaces;

public interface IWelfareApplicationRepository : IRepository<WelfareApplication>
{
    Task<IEnumerable<WelfareApplication>> GetByStatusAsync(string status);
    Task<IEnumerable<WelfareApplication>> GetByCitizenIdAsync(int citizenId);
    Task<IEnumerable<WelfareApplication>> GetByProgramIdAsync(int programId);
    Task<IEnumerable<WelfareApplication>> GetPendingApplicationsAsync();
    Task<IEnumerable<WelfareApplication>> GetApplicationsByDateRangeAsync(DateOnly startDate, DateOnly endDate);
    Task<bool> UpdateStatusAsync(int applicationId, string status);
}
