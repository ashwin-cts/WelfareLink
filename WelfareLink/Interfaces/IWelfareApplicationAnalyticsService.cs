namespace WelfareLink.Interfaces
{
    public interface IWelfareApplicationAnalyticsService
    {
        Task<Dictionary<string, object>> GetDashboardMetricsAsync();
        Task<Dictionary<string, int>> GetStatusBreakdownAsync();
        Task<Dictionary<string, object>> GetMonthlyTrendsAsync(int year);
        Task<Dictionary<string, object>> GetEligibilityReportAsync();
        Task<double> GetApprovalRateAsync();
        Task<int> GetAverageProcessingDaysAsync();
    }
}
