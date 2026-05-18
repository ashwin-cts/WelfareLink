using WelfareLink.ProgramManagement.API.Models;

namespace WelfareLink.ProgramManagement.API.Interfaces;

public interface IEligibilityCheckRepository : IRepository<EligibilityCheck>
{
    Task<IEnumerable<EligibilityCheck>> GetByApplicationIdAsync(int applicationId);
    Task<IEnumerable<EligibilityCheck>> GetByOfficerIdAsync(int officerId);
    Task<IEnumerable<EligibilityCheck>> GetByResultAsync(string result);
    Task<IEnumerable<EligibilityCheck>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate);
    Task<EligibilityCheck?> GetLatestCheckForApplicationAsync(int applicationId);
}
