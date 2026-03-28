using Microsoft.AspNetCore.Mvc;
using WelfareLink.Interfaces;
using WelfareLink.Models;
using WelfareLink.Services;
namespace WelfareLink.Controllers
{
    
    public class WelfareApplicationAnalyticsController : Controller
    {
        private readonly IWelfareApplicationAnalyticsService _analyticsService;

        public WelfareApplicationAnalyticsController(IWelfareApplicationAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }
        // GET: WelfareApplicationAnalytics
        // WelfareApplication Analytics Dashboard (Manager)
        // Displays metrics: applications submitted, pending, approved, rejected per programme
        public async Task<IActionResult> Index()
        {
            var metrics = await _analyticsService.GetDashboardMetricsAsync();

            // Pass data to view
            ViewBag.TotalApplications = metrics["TotalApplications"];
            ViewBag.PendingApplications = metrics["PendingApplications"];
            ViewBag.ApprovedApplications = metrics["ApprovedApplications"];
            ViewBag.RejectedApplications = metrics["RejectedApplications"];
            ViewBag.UnderReviewApplications = metrics["UnderReviewApplications"];
            ViewBag.ApprovalRate = metrics["ApprovalRate"];
            ViewBag.TotalChecks = metrics["TotalChecks"];
            ViewBag.EligibleChecks = metrics["EligibleChecks"];
            ViewBag.IneligibleChecks = metrics["IneligibleChecks"];
            ViewBag.ApplicationsByMonth = metrics["ApplicationsByMonth"];

            return View();
        }
        public async Task<IActionResult> StatusBreakdown()
        {
            var statusBreakdown = await _analyticsService.GetStatusBreakdownAsync();

            var statusData = statusBreakdown
                .Select(kvp => new
                {
                    Status = kvp.Key,
                    Count = kvp.Value,
                    Percentage = statusBreakdown.Values.Sum() > 0
                        ? (double)kvp.Value / statusBreakdown.Values.Sum() * 100
                        : 0
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            ViewBag.StatusBreakdown = statusData;
            return View();
        }
        // GET: WelfareApplicationAnalytics/MonthlyTrends
        // Get monthly application trends
        public async Task<IActionResult> MonthlyTrends(int? year)
        {
            var targetYear = year ?? DateTime.Now.Year;
            var trends = await _analyticsService.GetMonthlyTrendsAsync(targetYear);

            ViewBag.Year = trends["Year"];
            ViewBag.MonthlyData = trends["MonthlyData"];
            return View();
        }

        // GET: WelfareApplicationAnalytics/EligibilityReport
        // Eligibility check performance report
        public async Task<IActionResult> EligibilityReport()
        {
            var report = await _analyticsService.GetEligibilityReportAsync();

            ViewBag.ResultBreakdown = report["ResultBreakdown"];
            ViewBag.ChecksByMonth = report["ChecksByMonth"];
            ViewBag.TotalApplicationsChecked = report["TotalApplicationsChecked"];
            return View();
        }
        // GET: WelfareApplicationAnalytics/Export
        // Export analytics data (placeholder for future implementation)
        public async Task<IActionResult> Export(string format = "csv")
        {
            // TODO: Implement export functionality (CSV, Excel, PDF)
            TempData["InfoMessage"] = "Export functionality will be implemented soon.";
            return RedirectToAction(nameof(Index));
        }
    }
}
