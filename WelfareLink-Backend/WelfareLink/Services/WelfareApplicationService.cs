using WelfareLink.Interfaces;
using WelfareLink.Models;

namespace WelfareLink.Services;

public class WelfareApplicationService : IWelfareApplicationService
{
    private readonly IWelfareApplicationRepository _applicationRepository;
    private readonly IBenefitService _benefitService;

    public WelfareApplicationService(IWelfareApplicationRepository applicationRepository, IBenefitService benefitService)
    {
        _applicationRepository = applicationRepository;
        _benefitService = benefitService;
    }

    public async Task<IEnumerable<WelfareApplication>> GetAllApplicationsAsync()
    {
        return await _applicationRepository.GetAllAsync();
    }

    public async Task<WelfareApplication?> GetApplicationByIdAsync(int id)
    {
        return await _applicationRepository.GetByIdAsync(id);
    }

    public async Task<WelfareApplication> CreateApplicationAsync(WelfareApplication application)
    {
        // Business logic validation
        if (application == null)
        {
            throw new ArgumentNullException(nameof(application));
        }

        // Set default values
        application.SubmittedDate = DateOnly.FromDateTime(DateTime.Now);
        application.Status = "Pending";

        return await _applicationRepository.AddAsync(application);
    }

    public async Task UpdateApplicationAsync(WelfareApplication application)
    {
        if (application == null)
        {
            throw new ArgumentNullException(nameof(application));
        }

        if (!await _applicationRepository.ExistsAsync(application.ApplicationID))
        {
            throw new InvalidOperationException($"Application with ID {application.ApplicationID} not found.");
        }

        await _applicationRepository.UpdateAsync(application);

        if (application.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
        {
            await _benefitService.CreateBenefitForApprovedApplicationAsync(application.ApplicationID);
        }
    }

    public async Task DeleteApplicationAsync(int id)
    {
        var application = await _applicationRepository.GetByIdAsync(id);
        if (application == null)
        {
            throw new InvalidOperationException($"Application with ID {id} not found.");
        }

        await _applicationRepository.DeleteAsync(id);
    }

    public async Task<bool> ApplicationExistsAsync(int id)
    {
        return await _applicationRepository.ExistsAsync(id);
    }

    public async Task<IEnumerable<WelfareApplication>> GetPendingApplicationsAsync()
    {
        return await _applicationRepository.GetPendingApplicationsAsync();
    }

    public async Task<IEnumerable<WelfareApplication>> GetApplicationsByStatusAsync(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            throw new ArgumentException("Status cannot be null or empty.", nameof(status));
        }

        return await _applicationRepository.GetByStatusAsync(status);
    }

    public async Task<IEnumerable<WelfareApplication>> GetApplicationsByDateRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        if (startDate > endDate)
        {
            throw new ArgumentException("Start date cannot be after end date.");
        }

        return await _applicationRepository.GetApplicationsByDateRangeAsync(startDate, endDate);
    }

    public async Task<bool> UpdateApplicationStatusAsync(int applicationId, string status)
    {
        // Validate status
        var validStatuses = new[] { "Pending", "Under Review", "Approved", "Rejected", "Fully Disbursed" };
        if (!validStatuses.Contains(status))
        {
            throw new ArgumentException($"Invalid status. Must be one of: {string.Join(", ", validStatuses)}");
        }

        var result = await _applicationRepository.UpdateStatusAsync(applicationId, status);

        if (result && status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
        {
            await _benefitService.CreateBenefitForApprovedApplicationAsync(applicationId);
        }

        return result;
    }

    public async Task<bool> SubmitApplicationAsync(WelfareApplication application)
    {
        if (application == null)
        {
            return false;
        }

        // Business logic: Validate application data
        application.SubmittedDate = DateOnly.FromDateTime(DateTime.Now);
        application.Status = "Pending";

        var createdApplication = await _applicationRepository.AddAsync(application);

        // TODO: Send notification to citizen confirming submission
        // TODO: Assign application to available officer

        return createdApplication != null;
    }

    public async Task<Dictionary<string, int>> GetApplicationStatusSummaryAsync()
    {
        var allApplications = await _applicationRepository.GetAllAsync();

        return allApplications
            .GroupBy(a => a.Status)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<int> GetPendingApplicationCountAsync()
    {
        var pendingApplications = await _applicationRepository.GetPendingApplicationsAsync();
        return pendingApplications.Count();
    }
}
