using Microsoft.AspNetCore.Mvc;
using WelfareLink.Services;
using System.Text.Json;

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

            // Set default values if no data is returned
            ViewBag.TotalApplications = 0;
            ViewBag.PendingApplications = 0;
            ViewBag.ApprovedApplications = 0;
            ViewBag.RejectedApplications = 0;
            ViewBag.ApplicationsByMonth = new List<object>();
            ViewBag.StatusBreakdown = new List<object>();

            if (metrics != null && metrics.Count > 0)
            {
                foreach (var kvp in metrics)
                {
                    // Convert JsonElement values to proper types to avoid runtime binder exceptions
                    var value = kvp.Value;
                    if (value is System.Text.Json.JsonElement jsonElement)
                    {
                        if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.Number)
                        {
                            if (jsonElement.TryGetInt32(out int intVal))
                                value = intVal;
                            else if (jsonElement.TryGetDouble(out double doubleVal))
                                value = doubleVal;
                        }
                        else if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.String)
                        {
                            value = jsonElement.GetString();
                        }
                        else if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.True || jsonElement.ValueKind == System.Text.Json.JsonValueKind.False)
                        {
                            value = jsonElement.GetBoolean();
                        }
                    }
                    ViewBag[kvp.Key] = value;
                }
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
                ViewBag.Year = ConvertJsonElement(trends.GetValueOrDefault("Year"));
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
                ViewBag.TotalApplicationsChecked = ConvertJsonElement(report.GetValueOrDefault("TotalApplicationsChecked"));
            }
            return View();
        }

        // GET: WelfareApplicationAnalytics/Export
        public IActionResult Export(string format = "csv")
        {
            TempData["InfoMessage"] = "Export functionality will be implemented soon.";
            return RedirectToAction(nameof(Index));
        }

        // Helper method to convert JsonElement to appropriate type
        private static object? ConvertJsonElement(object? value)
        {
            if (value is JsonElement jsonElement)
            {
                if (jsonElement.ValueKind == JsonValueKind.Number)
                {
                    if (jsonElement.TryGetInt32(out int intVal))
                        return intVal;
                    else if (jsonElement.TryGetDouble(out double doubleVal))
                        return doubleVal;
                }
                else if (jsonElement.ValueKind == JsonValueKind.String)
                {
                    return jsonElement.GetString();
                }
                else if (jsonElement.ValueKind == JsonValueKind.True || jsonElement.ValueKind == JsonValueKind.False)
                {
                    return jsonElement.GetBoolean();
                }
                else if (jsonElement.ValueKind == JsonValueKind.Null)
                {
                    return null;
                }
            }
            return value;
        }
    }
}
