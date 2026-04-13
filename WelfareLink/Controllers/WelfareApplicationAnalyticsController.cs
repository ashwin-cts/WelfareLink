using Microsoft.AspNetCore.Mvc;
using WelfareLink.Services;

namespace WelfareLink.Controllers
{
    public class WelfareApplicationAnalyticsController : Controller
    {
        private readonly WelfareApiClient _api;

        public WelfareApplicationAnalyticsController(WelfareApiClient api)
        {
            _api = api;
        }

        // GET: WelfareApplicationAnalytics
        public async Task<IActionResult> Index()
        {
            var metrics = await _api.GetApplicationAnalyticsDashboardAsync();
            if (metrics != null)
            {
                foreach (var kvp in metrics)
                    ViewData[kvp.Key] = kvp.Value;
            }
            return View();
        }

        public async Task<IActionResult> StatusBreakdown()
        {
            var statusData = await _api.GetApplicationStatusBreakdownAsync();
            ViewBag.StatusBreakdown = statusData;
            return View();
        }

        // GET: WelfareApplicationAnalytics/MonthlyTrends
        public async Task<IActionResult> MonthlyTrends(int? year)
        {
            var targetYear = year ?? DateTime.Now.Year;
            var trends = await _api.GetApplicationMonthlyTrendsAsync(targetYear);
            if (trends != null)
            {
                ViewBag.Year = trends.GetValueOrDefault("Year");
                ViewBag.MonthlyData = trends.GetValueOrDefault("MonthlyData");
            }
            return View();
        }

        // GET: WelfareApplicationAnalytics/EligibilityReport
        public async Task<IActionResult> EligibilityReport()
        {
            var report = await _api.GetEligibilityReportAsync();
            if (report != null)
            {
                ViewBag.ResultBreakdown = report.GetValueOrDefault("ResultBreakdown");
                ViewBag.ChecksByMonth = report.GetValueOrDefault("ChecksByMonth");
                ViewBag.TotalApplicationsChecked = report.GetValueOrDefault("TotalApplicationsChecked");
            }
            return View();
        }

        // GET: WelfareApplicationAnalytics/Export
        public IActionResult Export(string format = "csv")
        {
            TempData["InfoMessage"] = "Export functionality will be implemented soon.";
            return RedirectToAction(nameof(Index));
        }
    }
}
