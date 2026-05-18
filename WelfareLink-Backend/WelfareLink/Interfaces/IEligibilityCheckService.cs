using WelfareLink.Models;

namespace WelfareLink.Interfaces;

public interface IEligibilityCheckService
{
    Task<IEnumerable<EligibilityCheck>> GetAllChecksAsync();
    Task<EligibilityCheck?> GetCheckByIdAsync(int id);
    Task<EligibilityCheck> CreateCheckAsync(EligibilityCheck check, int? applicationId = null);
    Task UpdateCheckAsync(EligibilityCheck check);
    Task DeleteCheckAsync(int id);
    Task<bool> CheckExistsAsync(int id);

    // Business logic methods
    Task<IEnumerable<EligibilityCheck>> GetChecksByApplicationIdAsync(int applicationId);
    Task<IEnumerable<EligibilityCheck>> GetChecksByResultAsync(string result);
    Task<IEnumerable<EligibilityCheck>> GetChecksByDateRangeAsync(DateOnly startDate, DateOnly endDate);
    Task<EligibilityCheck?> GetLatestCheckForApplicationAsync(int applicationId);
    Task<bool> PerformEligibilityCheckAsync(int applicationId, string result, string resultCode, string notes);
    Task<Dictionary<string, int>> GetEligibilityResultSummaryAsync();
    Task<double> GetEligibilityRateAsync();
}
