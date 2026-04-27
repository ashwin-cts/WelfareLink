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
            try
            {
                // Initialize ViewBag with default values to prevent null reference exceptions in view
                ViewBag.TotalApplications = 0;
                ViewBag.PendingApplications = 0;
                ViewBag.ApprovedApplications = 0;
                ViewBag.RejectedApplications = 0;
                ViewBag.UnderReviewApplications = 0;
                ViewBag.ApprovalRate = 0;
                ViewBag.TotalChecks = 0;
                ViewBag.EligibleChecks = 0;
                ViewBag.IneligibleChecks = 0;
                ViewBag.ApplicationsByMonth = new List<dynamic>();

                var metrics = await _api.GetApplicationAnalyticsDashboardAsync();

                // If analytics API returns null or empty, build metrics from actual data
                if (metrics == null || metrics.Count == 0)
                {
                    var applications = await _api.GetAllApplicationsAsync();
                    var appList = applications.ToList();

                    int approved = appList.Count(a => a.Status == "Approved");
                    int total = appList.Count;

                    metrics = new Dictionary<string, object>
                    {
                        { "TotalApplications", total },
                        { "PendingApplications", appList.Count(a => a.Status == "Pending") },
                        { "ApprovedApplications", approved },
                        { "RejectedApplications", appList.Count(a => a.Status == "Rejected") },
                        { "UnderReviewApplications", appList.Count(a => a.Status == "Under Review") },
                        { "ApprovalRate", total > 0 ? Math.Round((double)approved / total * 100, 2) : 0 }
                    };

                    // Add eligibility check count and breakdown
                    try
                    {
                        var checks = await _api.GetAllChecksAsync();
                        var checkList = checks.ToList();
                        metrics["TotalChecks"] = checkList.Count;

                        // Calculate eligible vs ineligible checks
                        var eligibleCount = checkList.Count(c => c.Result == "Eligible" || c.Result == "Pass");
                        var ineligibleCount = checkList.Count(c => c.Result == "Ineligible" || c.Result == "Fail");

                        ViewBag.EligibleChecks = eligibleCount;
                        ViewBag.IneligibleChecks = ineligibleCount;
                    }
                    catch
                    {
                        metrics["TotalChecks"] = 0;
                        ViewBag.EligibleChecks = 0;
                        ViewBag.IneligibleChecks = 0;
                    }

                    // Build applications by month
                    try
                    {
                        var monthlyData = appList
                            .GroupBy(a => new { Month = a.SubmittedDate.ToString("MMMM yyyy"), Year = a.SubmittedDate.Year })
                            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                            .Select(g => new { Month = g.Key.Month, Count = g.Count() })
                            .ToList();

                        ViewBag.ApplicationsByMonth = monthlyData;
                    }
                    catch
                    {
                        ViewBag.ApplicationsByMonth = new List<dynamic>();
                    }
                }
                else
                {
                    // Analytics API returned data, but we still need to populate EligibleChecks and IneligibleChecks if not present
                    if (!metrics.ContainsKey("EligibleChecks") || !metrics.ContainsKey("IneligibleChecks"))
                    {
                        try
                        {
                            var checks = await _api.GetAllChecksAsync();
                            var checkList = checks.ToList();

                            var eligibleCount = checkList.Count(c => c.Result == "Eligible" || c.Result == "Pass");
                            var ineligibleCount = checkList.Count(c => c.Result == "Ineligible" || c.Result == "Fail");

                            metrics["EligibleChecks"] = eligibleCount;
                            metrics["IneligibleChecks"] = ineligibleCount;
                        }
                        catch
                        {
                            metrics["EligibleChecks"] = 0;
                            metrics["IneligibleChecks"] = 0;
                        }
                    }

                    // Build applications by month if not present
                    if (!metrics.ContainsKey("ApplicationsByMonth"))
                    {
                        try
                        {
                            var applications = await _api.GetAllApplicationsAsync();
                            var appList = applications.ToList();

                            var monthlyData = appList
                                .GroupBy(a => new { Month = a.SubmittedDate.ToString("MMMM yyyy"), Year = a.SubmittedDate.Year })
                                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                                .Select(g => new { Month = g.Key.Month, Count = g.Count() })
                                .ToList();

                            metrics["ApplicationsByMonth"] = monthlyData;
                        }
                        catch
                        {
                            metrics["ApplicationsByMonth"] = new List<dynamic>();
                        }
                    }
                }

                if (metrics != null)
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
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error loading analytics: {ex.Message}";
                return View();
            }
        }

        public async Task<IActionResult> StatusBreakdown()
        {
            try
            {
                var statusData = await _api.GetApplicationStatusBreakdownAsync();
                var statusList = statusData?.ToList() ?? new List<StatusBreakdownItem>();

                // If API returns empty, build from actual application data
                if (statusList.Count == 0)
                {
                    var applications = await _api.GetAllApplicationsAsync();
                    var appList = applications.ToList();

                    statusList = appList
                        .GroupBy(a => a.Status)
                        .Select(g => new StatusBreakdownItem
                        {
                            Status = g.Key,
                            Count = g.Count(),
                            Percentage = appList.Count > 0 ? Math.Round((double)g.Count() / appList.Count * 100, 2) : 0
                        })
                        .ToList();
                }

                ViewBag.StatusBreakdown = statusList;
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error loading status breakdown: {ex.Message}";
                ViewBag.StatusBreakdown = new List<StatusBreakdownItem>();
                return View();
            }
        }

        // GET: WelfareApplicationAnalytics/MonthlyTrends
        public async Task<IActionResult> MonthlyTrends(int? year)
        {
            try
            {
                var targetYear = year ?? DateTime.Now.Year;
                var trends = await _api.GetApplicationMonthlyTrendsAsync(targetYear);

                // Initialize defaults
                ViewBag.Year = targetYear;
                ViewBag.MonthlyData = new List<dynamic>();

                if (trends != null && trends.Count > 0)
                {
                    ViewBag.Year = ConvertJsonElement(trends.GetValueOrDefault("Year")) ?? targetYear;
                    var monthlyData = trends.GetValueOrDefault("MonthlyData");

                    // Convert to list if it's an array or IEnumerable
                    if (monthlyData is System.Collections.IEnumerable enumerable && !(monthlyData is string))
                    {
                        ViewBag.MonthlyData = enumerable.Cast<dynamic>().ToList();
                    }
                    else
                    {
                        ViewBag.MonthlyData = monthlyData ?? new List<dynamic>();
                    }
                }
                else
                {
                    // Build from actual application data
                    var applications = await _api.GetAllApplicationsAsync();
                    var appList = applications.ToList();

                    var monthlyData = appList
                        .Where(a => a.SubmittedDate.Year == targetYear)
                        .GroupBy(a => a.SubmittedDate.Month)
                        .OrderBy(g => g.Key)
                        .Select(g => new
                        {
                            Month = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key),
                            Total = g.Count(),
                            Pending = g.Count(a => a.Status == "Pending"),
                            Approved = g.Count(a => a.Status == "Approved"),
                            Rejected = g.Count(a => a.Status == "Rejected"),
                            UnderReview = g.Count(a => a.Status == "Under Review")
                        })
                        .Cast<dynamic>()
                        .ToList();

                    ViewBag.MonthlyData = monthlyData;
                }

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error loading monthly trends: {ex.Message}";
                ViewBag.MonthlyData = new List<dynamic>();
                ViewBag.Year = year ?? DateTime.Now.Year;
                return View();
            }
        }

        // GET: WelfareApplicationAnalytics/EligibilityReport
        public async Task<IActionResult> EligibilityReport()
        {
            try
            {
                var report = await _api.GetEligibilityReportAsync();

                // Initialize defaults
                ViewBag.ResultBreakdown = new List<dynamic>();
                ViewBag.ChecksByMonth = new List<dynamic>();
                ViewBag.TotalApplicationsChecked = 0;

                if (report != null && report.Count > 0)
                {
                    // Try to populate from API response
                    var resultBreakdown = report.GetValueOrDefault("ResultBreakdown");
                    var checksByMonth = report.GetValueOrDefault("ChecksByMonth");
                    ViewBag.TotalApplicationsChecked = ConvertJsonElement(report.GetValueOrDefault("TotalApplicationsChecked")) ?? 0;

                    // Convert to lists if they're enumerables
                    if (resultBreakdown is System.Collections.IEnumerable rb && !(rb is string))
                    {
                        ViewBag.ResultBreakdown = rb.Cast<dynamic>().ToList();
                    }
                    if (checksByMonth is System.Collections.IEnumerable cm && !(cm is string))
                    {
                        ViewBag.ChecksByMonth = cm.Cast<dynamic>().ToList();
                    }
                }
                else
                {
                    // Build from actual check data
                    var checks = await _api.GetAllChecksAsync();
                    var checkList = checks.ToList();

                    // Result breakdown
                    var resultBreakdown = checkList
                        .GroupBy(c => c.Result)
                        .Select(g => new { Result = g.Key, Count = g.Count() })
                        .Cast<dynamic>()
                        .ToList();

                    ViewBag.ResultBreakdown = resultBreakdown;
                    ViewBag.TotalApplicationsChecked = checkList.Select(c => c.ApplicationID).Distinct().Count();

                    // Checks by month
                    var checksByMonth = checkList
                        .GroupBy(c => c.Date.Month)
                        .OrderBy(g => g.Key)
                        .Select(g => new { Month = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key), Count = g.Count() })
                        .Cast<dynamic>()
                        .ToList();

                    ViewBag.ChecksByMonth = checksByMonth;
                }

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error loading eligibility report: {ex.Message}";
                ViewBag.ResultBreakdown = new List<dynamic>();
                ViewBag.ChecksByMonth = new List<dynamic>();
                ViewBag.TotalApplicationsChecked = 0;
                return View();
            }
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
