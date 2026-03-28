using WelfareLink.Interfaces;

namespace WelfareLink.Services
{
    public class WelfareApplicationAnalyticsService : IWelfareApplicationAnalyticsService
    {
        private readonly IWelfareApplicationRepository _applicationRepository;
        private readonly IEligibilityCheckRepository _eligibilityCheckRepository;

        public WelfareApplicationAnalyticsService(
            IWelfareApplicationRepository applicationRepository,
            IEligibilityCheckRepository eligibilityCheckRepository)
        {
            _applicationRepository = applicationRepository;
            _eligibilityCheckRepository = eligibilityCheckRepository;
        }

        public async Task<Dictionary<string, object>> GetDashboardMetricsAsync()
        {
            var allApplications = await _applicationRepository.GetAllAsync();
            var allChecks = await _eligibilityCheckRepository.GetAllAsync();

            var totalApplications = allApplications.Count();
            var pendingApplications = allApplications.Count(a => a.Status == "Pending");
            var approvedApplications = allApplications.Count(a => a.Status == "Approved");
            var rejectedApplications = allApplications.Count(a => a.Status == "Rejected");
            var underReviewApplications = allApplications.Count(a => a.Status == "Under Review");

            var processedApplications = approvedApplications + rejectedApplications;
            var approvalRate = processedApplications > 0
                ? (double)approvedApplications / processedApplications * 100
                : 0;

            var totalChecks = allChecks.Count();
            var eligibleChecks = allChecks.Count(c => c.Result.ToLower() == "eligible");
            var ineligibleChecks = allChecks.Count(c => c.Result.ToLower() == "ineligible");

            var applicationsByMonth = allApplications
                .GroupBy(a => new { a.SubmittedDate.Year, a.SubmittedDate.Month })
                .Select(g => new
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    Count = g.Count()
                })
                .OrderBy(x => x.Month)
                .ToList();

            return new Dictionary<string, object>
            {
                { "TotalApplications", totalApplications },
                { "PendingApplications", pendingApplications },
                { "ApprovedApplications", approvedApplications },
                { "RejectedApplications", rejectedApplications },
                { "UnderReviewApplications", underReviewApplications },
                { "ApprovalRate", Math.Round(approvalRate, 2) },
                { "TotalChecks", totalChecks },
                { "EligibleChecks", eligibleChecks },
                { "IneligibleChecks", ineligibleChecks },
                { "ApplicationsByMonth", applicationsByMonth }
            };
        }

        public async Task<Dictionary<string, int>> GetStatusBreakdownAsync()
        {
            var allApplications = await _applicationRepository.GetAllAsync();

            return allApplications
                .GroupBy(a => a.Status)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<Dictionary<string, object>> GetMonthlyTrendsAsync(int year)
        {
            var allApplications = await _applicationRepository.GetAllAsync();

            var monthlyData = allApplications
                .Where(a => a.SubmittedDate.Year == year)
                .GroupBy(a => a.SubmittedDate.Month)
                .Select(g => new
                {
                    Month = new DateTime(year, g.Key, 1).ToString("MMMM"),
                    Total = g.Count(),
                    Pending = g.Count(a => a.Status == "Pending"),
                    Approved = g.Count(a => a.Status == "Approved"),
                    Rejected = g.Count(a => a.Status == "Rejected"),
                    UnderReview = g.Count(a => a.Status == "Under Review")
                })
                .OrderBy(x => x.Month)
                .ToList();

            return new Dictionary<string, object>
            {
                { "Year", year },
                { "MonthlyData", monthlyData }
            };
        }

        public async Task<Dictionary<string, object>> GetEligibilityReportAsync()
        {
            var allChecks = await _eligibilityCheckRepository.GetAllAsync();

            // Take only the latest check per application so re-checked applications
            // are represented by their final (most recent) result only.
            var latestPerApplication = allChecks
                .GroupBy(c => c.ApplicationID)
                .Select(g => g.OrderByDescending(c => c.Date).First())
                .ToList();

            var totalApplicationsChecked = latestPerApplication.Count;

            var resultBreakdown = latestPerApplication
                .GroupBy(c => c.Result)
                .Select(g => new
                {
                    Result = g.Key,
                    Count = g.Count(),
                    Percentage = totalApplicationsChecked > 0
                        ? (double)g.Count() / totalApplicationsChecked * 100
                        : 0
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            var checksByMonth = latestPerApplication
                .GroupBy(c => new { c.Date.Year, c.Date.Month })
                .Select(g => new
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    Total = g.Count(),
                    Eligible = g.Count(c => c.Result.Equals("Eligible", StringComparison.OrdinalIgnoreCase)),
                    Ineligible = g.Count(c => c.Result.Equals("Ineligible", StringComparison.OrdinalIgnoreCase))
                })
                .OrderBy(x => x.Month)
                .ToList();

            return new Dictionary<string, object>
            {
                { "ResultBreakdown", resultBreakdown },
                { "ChecksByMonth", checksByMonth },
                { "TotalApplicationsChecked", totalApplicationsChecked }
            };
        }

        public async Task<double> GetApprovalRateAsync()
        {
            var allApplications = await _applicationRepository.GetAllAsync();

            var approvedCount = allApplications.Count(a => a.Status == "Approved");
            var rejectedCount = allApplications.Count(a => a.Status == "Rejected");
            var processedCount = approvedCount + rejectedCount;

            if (processedCount == 0)
            {
                return 0;
            }

            return (double)approvedCount / processedCount * 100;
        }

        public async Task<int> GetAverageProcessingDaysAsync()
        {
            var completedApplications = (await _applicationRepository.GetAllAsync())
                .Where(a => a.Status == "Approved" || a.Status == "Rejected")
                .ToList();

            if (!completedApplications.Any())
            {
                return 0;
            }

            // Calculate days from submission to now (placeholder logic)
            // In a real scenario, you'd have a CompletedDate field
            var totalDays = completedApplications
                .Select(a => DateTime.Now.Subtract(a.SubmittedDate.ToDateTime(TimeOnly.MinValue)).Days)
                .Sum();

            return totalDays / completedApplications.Count;
        }
    }
}
