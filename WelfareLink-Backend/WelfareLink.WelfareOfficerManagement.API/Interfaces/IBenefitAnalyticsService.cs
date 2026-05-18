using WelfareLink.WelfareOfficerManagement.API.ViewModels;

namespace WelfareLink.WelfareOfficerManagement.API.Interfaces
{
    public interface IBenefitAnalyticsService
    {
        Task<AnalyticsDashboardViewModel> GetDashboardDataAsync();
        Task<int> GetTotalAllocatedCountAsync();
        Task<int> GetTotalDisbursedCountAsync();
        Task<int> GetTotalPendingCountAsync();
        Task<double> GetTotalAmountAllocatedAsync();
        Task<double> GetDisbursementEfficiencyAsync();
        Task<List<BenefitTypeBreakdown>> GetBenefitTypeBreakdownsAsync();
        Task<List<RecentDisbursement>> GetRecentDisbursementsAsync(int count = 5);
        Task<List<MonthlyTrend>> GetMonthlyTrendsAsync(int months = 6);
    }
}
