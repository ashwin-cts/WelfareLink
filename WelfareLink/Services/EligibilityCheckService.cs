using WelfareLink.Interfaces;
using WelfareLink.Models;

namespace WelfareLink.Services;

public class EligibilityCheckService : IEligibilityCheckService
{
    private readonly IEligibilityCheckRepository _eligibilityCheckRepository;
    private readonly IWelfareApplicationRepository _applicationRepository;

    public EligibilityCheckService(
        IEligibilityCheckRepository eligibilityCheckRepository,
        IWelfareApplicationRepository applicationRepository)
    {
        _eligibilityCheckRepository = eligibilityCheckRepository;
        _applicationRepository = applicationRepository;
    }

    public async Task<IEnumerable<EligibilityCheck>> GetAllChecksAsync()
    {
        return await _eligibilityCheckRepository.GetAllAsync();
    }

    public async Task<EligibilityCheck?> GetCheckByIdAsync(int id)
    {
        return await _eligibilityCheckRepository.GetByIdAsync(id);
    }

    public async Task<EligibilityCheck> CreateCheckAsync(EligibilityCheck check, int? applicationId = null)
    {
        if (check == null)
        {
            throw new ArgumentNullException(nameof(check));
        }

        // Validate result
        var validResults = new[] { "Eligible", "Ineligible" };
        if (!validResults.Contains(check.Result, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Result must be either 'Eligible' or 'Ineligible'.");
        }

        // Set the date if not already set
        check.Date = DateOnly.FromDateTime(DateTime.Now);

        // Create the eligibility check
        var createdCheck = await _eligibilityCheckRepository.AddAsync(check);

        // Update application status if applicationId is provided
        if (applicationId.HasValue)
        {
            string newStatus = check.Result.ToLower() == "eligible" ? "Approved" : "Rejected";
            await _applicationRepository.UpdateStatusAsync(applicationId.Value, newStatus);

            // TODO: Send notification to citizen with outcome
        }

        return createdCheck;
    }

    public async Task UpdateCheckAsync(EligibilityCheck check)
    {
        if (check == null)
        {
            throw new ArgumentNullException(nameof(check));
        }

        var existingCheck = await _eligibilityCheckRepository.GetByIdAsync(check.CheckID);
        if (existingCheck == null)
        {
            throw new InvalidOperationException($"Eligibility check with ID {check.CheckID} not found.");
        }

        await _eligibilityCheckRepository.UpdateAsync(check);
    }

    public async Task DeleteCheckAsync(int id)
    {
        var check = await _eligibilityCheckRepository.GetByIdAsync(id);
        if (check == null)
        {
            throw new InvalidOperationException($"Eligibility check with ID {id} not found.");
        }

        await _eligibilityCheckRepository.DeleteAsync(id);
    }

    public async Task<bool> CheckExistsAsync(int id)
    {
        return await _eligibilityCheckRepository.ExistsAsync(id);
    }

    public async Task<IEnumerable<EligibilityCheck>> GetChecksByApplicationIdAsync(int applicationId)
    {
        return await _eligibilityCheckRepository.GetByApplicationIdAsync(applicationId);
    }

    public async Task<IEnumerable<EligibilityCheck>> GetChecksByResultAsync(string result)
    {
        if (string.IsNullOrWhiteSpace(result))
        {
            throw new ArgumentException("Result cannot be null or empty.", nameof(result));
        }

        return await _eligibilityCheckRepository.GetByResultAsync(result);
    }

    public async Task<IEnumerable<EligibilityCheck>> GetChecksByDateRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        if (startDate > endDate)
        {
            throw new ArgumentException("Start date cannot be after end date.");
        }

        return await _eligibilityCheckRepository.GetByDateRangeAsync(startDate, endDate);
    }

    public async Task<EligibilityCheck?> GetLatestCheckForApplicationAsync(int applicationId)
    {
        return await _eligibilityCheckRepository.GetLatestCheckForApplicationAsync(applicationId);
    }

    public async Task<bool> PerformEligibilityCheckAsync(int applicationId, string result, string resultCode, string notes)
    {
        // Validate application exists
        var application = await _applicationRepository.GetByIdAsync(applicationId);
        if (application == null)
        {
            return false;
        }

        // Create eligibility check
        var check = new EligibilityCheck
        {
            Result = result,
            ResultCode = resultCode,
            Date = DateOnly.FromDateTime(DateTime.Now),
            Notes = notes
        };

        // Validate and create check
        await CreateCheckAsync(check, applicationId);

        // TODO: Log eligibility check for audit trail
        // TODO: Send notification to citizen

        return true;
    }

    public async Task<Dictionary<string, int>> GetEligibilityResultSummaryAsync()
    {
        var allChecks = await _eligibilityCheckRepository.GetAllAsync();

        return allChecks
            .GroupBy(c => c.Result)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<double> GetEligibilityRateAsync()
    {
        var allChecks = await _eligibilityCheckRepository.GetAllAsync();
        var totalChecks = allChecks.Count();

        if (totalChecks == 0)
        {
            return 0;
        }

        var eligibleChecks = allChecks.Count(c => c.Result.ToLower() == "eligible");
        return (double)eligibleChecks / totalChecks * 100;
    }
}
