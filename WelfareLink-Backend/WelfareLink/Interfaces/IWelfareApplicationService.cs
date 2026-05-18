using WelfareLink.Models;

namespace WelfareLink.Interfaces;

    public interface IWelfareApplicationService
    {
        Task<IEnumerable<WelfareApplication>> GetAllApplicationsAsync();
        Task<WelfareApplication?> GetApplicationByIdAsync(int id);
        Task<WelfareApplication> CreateApplicationAsync(WelfareApplication application);
        Task UpdateApplicationAsync(WelfareApplication application);
        Task DeleteApplicationAsync(int id);
        Task<bool> ApplicationExistsAsync(int id);

        // Business logic methods
        Task<IEnumerable<WelfareApplication>> GetPendingApplicationsAsync();
        Task<IEnumerable<WelfareApplication>> GetApplicationsByStatusAsync(string status);
        Task<IEnumerable<WelfareApplication>> GetApplicationsByDateRangeAsync(DateOnly startDate, DateOnly endDate);
        Task<bool> UpdateApplicationStatusAsync(int applicationId, string status);
        Task<bool> SubmitApplicationAsync(WelfareApplication application);
        Task<Dictionary<string, int>> GetApplicationStatusSummaryAsync();
        Task<int> GetPendingApplicationCountAsync();
    }

