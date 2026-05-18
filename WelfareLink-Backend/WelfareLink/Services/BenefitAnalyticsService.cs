using WelfareLink.Interfaces;
using WelfareLink.Models;
using WelfareLink.ViewModels;

namespace WelfareLink.Services
{
    public class BenefitAnalyticsService : IBenefitAnalyticsService
    {
        private readonly IBenefitService _benefitService;
        private readonly IDisbursementService _disbursementService;

        public BenefitAnalyticsService(IBenefitService benefitService, IDisbursementService disbursementService)
        {
            _benefitService = benefitService;
            _disbursementService = disbursementService;
        }

        public async Task<AnalyticsDashboardViewModel> GetDashboardDataAsync()
        {
            var benefits = (await _benefitService.GetAllBenefitsAsync()).ToList();
            var disbursements = (await _disbursementService.GetAllDisbursementsAsync()).ToList();

            return new AnalyticsDashboardViewModel
            {
                // Summary Metrics
                TotalAllocated = benefits.Count,
                TotalDisbursed = CalculateCompletedDisbursements(disbursements),
                TotalPending = CalculatePendingDisbursements(disbursements),
                TotalFailed = CalculateFailedDisbursements(disbursements),

                // Financial Metrics
                TotalAmountAllocated = CalculateTotalAmountAllocated(benefits),
                TotalAmountDisbursed = CalculateTotalAmountDisbursed(benefits, disbursements),

                // Efficiency Metrics
                DisbursementEfficiency = CalculateDisbursementEfficiency(benefits, disbursements),
                AllocationRate = CalculateAllocationRate(benefits),

                // Breakdown and Trends
                BenefitTypeBreakdowns = CalculateBenefitTypeBreakdowns(benefits, disbursements),
                RecentDisbursements = GetRecentDisbursementsList(disbursements, 5),
                MonthlyTrends = CalculateMonthlyTrends(benefits, disbursements, 6)
            };
        }

        public async Task<int> GetTotalAllocatedCountAsync()
        {
            var benefits = await _benefitService.GetAllBenefitsAsync();
            return benefits.Count();
        }

        public async Task<int> GetTotalDisbursedCountAsync()
        {
            var disbursements = await _disbursementService.GetAllDisbursementsAsync();
            return CalculateCompletedDisbursements(disbursements.ToList());
        }

        public async Task<int> GetTotalPendingCountAsync()
        {
            var disbursements = await _disbursementService.GetAllDisbursementsAsync();
            return CalculatePendingDisbursements(disbursements.ToList());
        }

        public async Task<double> GetTotalAmountAllocatedAsync()
        {
            var benefits = await _benefitService.GetAllBenefitsAsync();
            return CalculateTotalAmountAllocated(benefits.ToList());
        }

        public async Task<double> GetDisbursementEfficiencyAsync()
        {
            var benefits = (await _benefitService.GetAllBenefitsAsync()).ToList();
            var disbursements = (await _disbursementService.GetAllDisbursementsAsync()).ToList();
            return CalculateDisbursementEfficiency(benefits, disbursements);
        }

        public async Task<List<BenefitTypeBreakdown>> GetBenefitTypeBreakdownsAsync()
        {
            var benefits = (await _benefitService.GetAllBenefitsAsync()).ToList();
            var disbursements = (await _disbursementService.GetAllDisbursementsAsync()).ToList();
            return CalculateBenefitTypeBreakdowns(benefits, disbursements);
        }

        public async Task<List<RecentDisbursement>> GetRecentDisbursementsAsync(int count = 5)
        {
            var disbursements = (await _disbursementService.GetAllDisbursementsAsync()).ToList();
            return GetRecentDisbursementsList(disbursements, count);
        }

        public async Task<List<MonthlyTrend>> GetMonthlyTrendsAsync(int months = 6)
        {
            var benefits = (await _benefitService.GetAllBenefitsAsync()).ToList();
            var disbursements = (await _disbursementService.GetAllDisbursementsAsync()).ToList();
            return CalculateMonthlyTrends(benefits, disbursements, months);
        }

        #region Private Calculation Methods

        private int CalculateCompletedDisbursements(List<Disbursement> disbursements)
        {
            return disbursements.Count(d => d.Status == "Completed");
        }

        private int CalculatePendingDisbursements(List<Disbursement> disbursements)
        {
            return disbursements.Count(d => d.Status == "Pending");
        }

        private int CalculateFailedDisbursements(List<Disbursement> disbursements)
        {
            return disbursements.Count(d => d.Status == "Failed");
        }

        private double CalculateTotalAmountAllocated(List<Benefit> benefits)
        {
            return benefits.Sum(b => b.Amount);
        }

        private double CalculateTotalAmountDisbursed(List<Benefit> benefits, List<Disbursement> disbursements)
        {
            // Sum the actual disbursement amounts for completed disbursements only
            return disbursements
                .Where(d => d.Status == "Completed")
                .Sum(d => d.Amount);
        }

        private double CalculateDisbursementEfficiency(List<Benefit> benefits, List<Disbursement> disbursements)
        {
            var totalAllocated = benefits.Where(b => b.Amount > 0).Sum(b => b.Amount);
            if (totalAllocated == 0)
                return 0;

            // Calculate efficiency based on actual disbursed amount vs allocated amount
            var totalDisbursed = disbursements
                .Where(d => d.Status == "Completed")
                .Sum(d => d.Amount);

            return Math.Round(totalDisbursed / totalAllocated * 100, 1);
        }

        private double CalculateAllocationRate(List<Benefit> benefits)
        {
            if (benefits.Count == 0)
                return 0;

            var allocatedCount = benefits.Count(b => b.Status == "Allocated" || b.Status == "Partially Disbursed" || b.Status == "Fully Disbursed");
            return Math.Round((double)allocatedCount / benefits.Count * 100, 1);
        }

        private List<BenefitTypeBreakdown> CalculateBenefitTypeBreakdowns(List<Benefit> benefits, List<Disbursement> disbursements)
        {
            return benefits
                .GroupBy(b => b.Type)
                .Select(g => 
                {
                    var benefitIds = g.Select(b => b.BenefitID).ToList();
                    var disbursedAmount = disbursements
                        .Where(d => benefitIds.Contains(d.BenefitID) && d.Status == "Completed")
                        .Sum(d => d.Amount);
                    var allocatedAmount = g.Sum(b => b.Amount);

                    return new BenefitTypeBreakdown
                    {
                        Type = g.Key,
                        Count = g.Count(),
                        TotalAmount = allocatedAmount,
                        DisbursedAmount = disbursedAmount,
                        DisbursedCount = disbursements.Count(d => benefitIds.Contains(d.BenefitID) && 
                                                                  d.Status == "Completed"),
                        Percentage = benefits.Count > 0 
                            ? Math.Round((double)g.Count() / benefits.Count * 100, 1) 
                            : 0
                    };
                })
                .OrderByDescending(b => b.Count)
                .ToList();
        }

        private List<RecentDisbursement> GetRecentDisbursementsList(List<Disbursement> disbursements, int count)
        {
            return disbursements
                .OrderByDescending(d => d.Date)
                .Take(count)
                .Select(d => new RecentDisbursement
                {
                    DisbursementID = d.DisbursementID,
                    BenefitType = d.Benefit?.Type ?? "N/A",
                    CitizenID = d.CitizenID,
                    Date = d.Date,
                    Status = d.Status
                })
                .ToList();
        }

        private List<MonthlyTrend> CalculateMonthlyTrends(List<Benefit> benefits, List<Disbursement> disbursements, int months)
        {
            var trends = new List<MonthlyTrend>();
            var today = DateTime.Today;

            for (int i = months - 1; i >= 0; i--)
            {
                var month = today.AddMonths(-i);
                var monthStart = new DateTime(month.Year, month.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                var monthlyAllocatedAmount = benefits
                    .Where(b => b.Date >= monthStart && b.Date <= monthEnd)
                    .Sum(b => b.Amount);

                var monthlyDisbursedAmount = disbursements
                    .Where(d => d.Date >= monthStart && d.Date <= monthEnd && d.Status == "Completed")
                    .Sum(d => d.Amount);

                trends.Add(new MonthlyTrend
                {
                    Month = month.ToString("MMM yyyy"),
                    Allocated = benefits.Count(b => b.Date >= monthStart && b.Date <= monthEnd),
                    Disbursed = disbursements.Count(d => d.Date >= monthStart && 
                                                         d.Date <= monthEnd && 
                                                         d.Status == "Completed"),
                    AllocatedAmount = monthlyAllocatedAmount,
                    DisbursedAmount = monthlyDisbursedAmount
                });
            }

            return trends;
        }

        #endregion
    }
}
