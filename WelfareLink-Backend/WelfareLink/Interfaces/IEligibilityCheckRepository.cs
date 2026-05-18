using WelfareLink.Models;

namespace WelfareLink.Interfaces;

public interface IEligibilityCheckRepository : IRepository<EligibilityCheck>
{
    Task<IEnumerable<EligibilityCheck>> GetByApplicationIdAsync(int applicationId);
    Task<IEnumerable<EligibilityCheck>> GetByOfficerIdAsync(int officerId);
    Task<IEnumerable<EligibilityCheck>> GetByResultAsync(string result);
    Task<IEnumerable<EligibilityCheck>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate);
    Task<EligibilityCheck?> GetLatestCheckForApplicationAsync(int applicationId);
}
