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

                    System.Diagnostics.Debug.WriteLine($"Total applications fetched: {appList.Count}");

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

                        System.Diagnostics.Debug.WriteLine($"Total checks fetched: {checkList.Count}");

                        // Calculate eligible vs ineligible checks
                        var eligibleCount = checkList.Count(c => c.Result == "Eligible" || c.Result == "Pass");
                        var ineligibleCount = checkList.Count(c => c.Result == "Ineligible" || c.Result == "Fail");

                        metrics["EligibleChecks"] = eligibleCount;
                        metrics["IneligibleChecks"] = ineligibleCount;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error fetching checks: {ex.Message}");
                        metrics["TotalChecks"] = 0;
                        metrics["EligibleChecks"] = 0;
                        metrics["IneligibleChecks"] = 0;
                    }

                    // Build applications by month
                    try
                    {
                        var monthlyData = appList
                            .GroupBy(a => a.SubmittedDate.Month)
                            .OrderBy(g => g.Key)
                            .Select(g => new
                            {
                                Month = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key),
                                Count = g.Count()
                            })
                            .ToList();

                        metrics["ApplicationsByMonth"] = monthlyData.Cast<dynamic>().ToList();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error building monthly data: {ex.Message}");
                        metrics["ApplicationsByMonth"] = new List<dynamic>();
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
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error fetching checks: {ex.Message}");
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
                                .GroupBy(a => a.SubmittedDate.Month)
                                .OrderBy(g => g.Key)
                                .Select(g => new
                                {
                                    Month = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key),
                                    Count = g.Count()
                                })
                                .ToList();

                            metrics["ApplicationsByMonth"] = monthlyData.Cast<dynamic>().ToList();
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error building monthly data: {ex.Message}");
                            metrics["ApplicationsByMonth"] = new List<dynamic>();
                        }
                    }
                }

                // Now populate ViewBag from metrics dictionary
                if (metrics != null && metrics.Count > 0)
                {
                    foreach (var kvp in metrics)
                    {
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
                            else if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                            {
                                try
                                {
                                    value = jsonElement.EnumerateArray().Cast<dynamic>().ToList();
                                }
                                catch
                                {
                                    value = new List<dynamic>();
                                }
                            }
                        }
                        else if (value is System.Collections.IEnumerable enumerable && !(value is string))
                        {
                            try
                            {
                                value = enumerable.Cast<dynamic>().ToList();
                            }
                            catch { }
                        }

                        ViewBag[kvp.Key] = value;
                        System.Diagnostics.Debug.WriteLine($"ViewBag.{kvp.Key} = {value}");
                    }
                }

                return View();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in Index: {ex}");
                ViewBag.ErrorMessage = $"Error loading analytics: {ex.Message}";
                return View();
            }
        }

        public async Task<IActionResult> StatusBreakdown()
        {
            try
            {
                var statusData = await _api.GetApplicationStatusBreakdownAsync();
                List<StatusBreakdownItem> statusList = new List<StatusBreakdownItem>();

                if (statusData != null)
                {
                    statusList = statusData.ToList();
                }

                // If API returns empty, build from actual application data
                if (statusList.Count == 0)
                {
                    try
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
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error building status breakdown from applications: {ex.Message}");
                    }
                }

                ViewBag.StatusBreakdown = statusList;
                ViewBag.HasStatusBreakdown = statusList.Count > 0;
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error loading status breakdown: {ex.Message}";
                ViewBag.StatusBreakdown = new List<StatusBreakdownItem>();
                ViewBag.HasStatusBreakdown = false;
                return View();
            }
        }

        // GET: WelfareApplicationAnalytics/MonthlyTrends
        public async Task<IActionResult> MonthlyTrends(int? year)
        {
            try
            {
                var targetYear = year ?? DateTime.Now.Year;
                System.Diagnostics.Debug.WriteLine($"MonthlyTrends called for year: {targetYear}");

                var trends = await _api.GetApplicationMonthlyTrendsAsync(targetYear);

                // Initialize defaults
                ViewBag.Year = targetYear;
                ViewBag.MonthlyData = new List<dynamic>();
                ViewBag.HasMonthlyData = false;

                List<dynamic> monthlyDataList = new List<dynamic>();

                if (trends != null && trends.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Trends API returned data with {trends.Count} entries");
                    ViewBag.Year = ConvertJsonElement(trends.GetValueOrDefault("Year")) ?? targetYear;
                    var monthlyData = trends.GetValueOrDefault("MonthlyData");

                    // Convert to list if it's an array or IEnumerable
                    if (monthlyData is System.Collections.IEnumerable enumerable && !(monthlyData is string))
                    {
                        // Properly deserialize JsonElement objects to ensure properties are accessible
                        monthlyDataList = ConvertJsonElementList(enumerable).ToList();
                        System.Diagnostics.Debug.WriteLine($"Converted {monthlyDataList.Count} monthly data items from API");
                    }
                    else if (monthlyData != null)
                    {
                        var converted = ConvertJsonElementToObject(monthlyData);
                        if (converted != null)
                        {
                            monthlyDataList = new List<dynamic> { converted };
                            System.Diagnostics.Debug.WriteLine("Converted single monthly data item from API");
                        }
                    }
                }

                // If no data from API, build from actual application data
                if (monthlyDataList.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("No data from API, building from application data");
                    try
                    {
                        var applications = await _api.GetAllApplicationsAsync();
                        var appList = applications.ToList();

                        System.Diagnostics.Debug.WriteLine($"Total applications fetched: {appList.Count}");

                        // Filter by year and group by month
                        var yearlApps = appList.Where(a => a.SubmittedDate.Year == targetYear).ToList();
                        System.Diagnostics.Debug.WriteLine($"Applications for {targetYear}: {yearlApps.Count}");

                        monthlyDataList = yearlApps
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

                        System.Diagnostics.Debug.WriteLine($"Built {monthlyDataList.Count} months of data");
                        foreach (var item in monthlyDataList)
                        {
                            System.Diagnostics.Debug.WriteLine($"  - {item.Month}: Total={item.Total}, Pending={item.Pending}, Approved={item.Approved}");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error building monthly data from applications: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"Exception: {ex}");
                    }
                }

                ViewBag.MonthlyData = monthlyDataList;
                ViewBag.HasMonthlyData = monthlyDataList.Count > 0;

                System.Diagnostics.Debug.WriteLine($"Final MonthlyData count: {monthlyDataList.Count}, HasMonthlyData: {ViewBag.HasMonthlyData}");

                return View();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in MonthlyTrends: {ex}");
                ViewBag.ErrorMessage = $"Error loading monthly trends: {ex.Message}";
                ViewBag.MonthlyData = new List<dynamic>();
                ViewBag.HasMonthlyData = false;
                ViewBag.Year = year ?? DateTime.Now.Year;
                return View();
            }
        }

        // GET: WelfareApplicationAnalytics/EligibilityReport
        public async Task<IActionResult> EligibilityReport()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("EligibilityReport action started");

                var report = await _api.GetEligibilityReportAsync();

                // Initialize defaults
                ViewBag.ResultBreakdown = new List<dynamic>();
                ViewBag.ChecksByMonth = new List<dynamic>();
                ViewBag.TotalApplicationsChecked = 0;
                ViewBag.HasResultBreakdown = false;
                ViewBag.HasChecksByMonth = false;

                List<dynamic> resultBreakdownList = new List<dynamic>();
                List<dynamic> checksByMonthList = new List<dynamic>();
                int totalApplicationsChecked = 0;

                if (report != null && report.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Report from API has {report.Count} entries");
                    // Try to populate from API response
                    var resultBreakdown = report.GetValueOrDefault("ResultBreakdown");
                    var checksByMonth = report.GetValueOrDefault("ChecksByMonth");
                    totalApplicationsChecked = (int?)ConvertJsonElement(report.GetValueOrDefault("TotalApplicationsChecked")) ?? 0;

                    // Convert ResultBreakdown to list
                    if (resultBreakdown is System.Collections.IEnumerable rb && !(rb is string))
                    {
                        try
                        {
                            resultBreakdownList = rb.Cast<dynamic>().ToList();
                            System.Diagnostics.Debug.WriteLine($"ResultBreakdown has {resultBreakdownList.Count} items");
                        }
                        catch { }
                    }

                    // Convert ChecksByMonth to list
                    if (checksByMonth is System.Collections.IEnumerable cm && !(cm is string))
                    {
                        try
                        {
                            checksByMonthList = cm.Cast<dynamic>().ToList();
                            System.Diagnostics.Debug.WriteLine($"ChecksByMonth has {checksByMonthList.Count} items");
                        }
                        catch { }
                    }
                }

                // If no data from API, build from actual check data
                if (resultBreakdownList.Count == 0 || checksByMonthList.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Building report from check data");
                    try
                    {
                        var checks = await _api.GetAllChecksAsync();
                        var checkList = checks.ToList();

                        System.Diagnostics.Debug.WriteLine($"Total checks fetched: {checkList.Count}");

                        if (checkList.Count > 0)
                        {
                            // Result breakdown
                            if (resultBreakdownList.Count == 0)
                            {
                                resultBreakdownList = checkList
                                    .GroupBy(c => c.Result)
                                    .Select(g => new
                                    {
                                        Result = g.Key,
                                        Count = g.Count(),
                                        Percentage = (double)g.Count() / checkList.Count * 100
                                    })
                                    .Cast<dynamic>()
                                    .ToList();

                                System.Diagnostics.Debug.WriteLine($"Result breakdown has {resultBreakdownList.Count} results");
                                foreach (var item in resultBreakdownList)
                                {
                                    System.Diagnostics.Debug.WriteLine($"  - {item.Result}: {item.Count} ({item.Percentage}%)");
                                }
                            }

                            // Total applications checked
                            if (totalApplicationsChecked == 0)
                            {
                                totalApplicationsChecked = checkList.Select(c => c.ApplicationID).Distinct().Count();
                                System.Diagnostics.Debug.WriteLine($"Total applications checked: {totalApplicationsChecked}");
                            }

                            // Checks by month
                            if (checksByMonthList.Count == 0)
                            {
                                checksByMonthList = checkList
                                    .GroupBy(c => c.Date.Month)
                                    .OrderBy(g => g.Key)
                                    .Select(g => new
                                    {
                                        Month = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key),
                                        Total = g.Count(),
                                        Eligible = g.Count(c => c.Result == "Eligible" || c.Result == "Pass"),
                                        Ineligible = g.Count(c => c.Result == "Ineligible" || c.Result == "Fail")
                                    })
                                    .Cast<dynamic>()
                                    .ToList();

                                System.Diagnostics.Debug.WriteLine($"Checks by month has {checksByMonthList.Count} months");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error building eligibility report from checks: {ex.Message}");
                    }
                }

                ViewBag.ResultBreakdown = resultBreakdownList;
                ViewBag.ChecksByMonth = checksByMonthList;
                ViewBag.TotalApplicationsChecked = totalApplicationsChecked;
                ViewBag.HasResultBreakdown = resultBreakdownList.Count > 0;
                ViewBag.HasChecksByMonth = checksByMonthList.Count > 0;

                System.Diagnostics.Debug.WriteLine($"Final result: {resultBreakdownList.Count} results, {checksByMonthList.Count} months, {totalApplicationsChecked} apps");

                return View();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in EligibilityReport: {ex}");
                ViewBag.ErrorMessage = $"Error loading eligibility report: {ex.Message}";
                ViewBag.ResultBreakdown = new List<dynamic>();
                ViewBag.ChecksByMonth = new List<dynamic>();
                ViewBag.TotalApplicationsChecked = 0;
                ViewBag.HasResultBreakdown = false;
                ViewBag.HasChecksByMonth = false;
                return View();
            }
        }

        // GET: WelfareApplicationAnalytics/Export
        public IActionResult Export(string format = "csv")
        {
            TempData["InfoMessage"] = "Export functionality will be implemented soon.";
            return RedirectToAction(nameof(Index));
        }

        // Helper method to safely check if a ViewBag collection has items
        private bool HasItems(dynamic collection)
        {
            try
            {
                return collection?.Count > 0;
            }
            catch
            {
                return false;
            }
        }

        // Helper method to convert JsonElement to dynamic object
        private dynamic ConvertJsonElementToObject(object item)
        {
            try
            {
                if (item is System.Text.Json.JsonElement jsonElement)
                {
                    var json = jsonElement.GetRawText();
                    return System.Text.Json.JsonSerializer.Deserialize<dynamic>(json, 
                        new System.Text.Json.JsonSerializerOptions 
                        { 
                            PropertyNameCaseInsensitive = true 
                        });
                }
                return item;
            }
            catch
            {
                return null;
            }
        }

        // Helper method to convert JsonElement list to dynamic list
        private IEnumerable<dynamic> ConvertJsonElementList(System.Collections.IEnumerable enumerable)
        {
            var result = new List<dynamic>();
            try
            {
                foreach (var item in enumerable)
                {
                    if (item is System.Text.Json.JsonElement jsonElement)
                    {
                        var json = jsonElement.GetRawText();
                        var obj = System.Text.Json.JsonSerializer.Deserialize<dynamic>(json,
                            new System.Text.Json.JsonSerializerOptions 
                            { 
                                PropertyNameCaseInsensitive = true 
                            });
                        if (obj != null)
                            result.Add(obj);
                    }
                    else
                    {
                        result.Add(item);
                    }
                }
            }
            catch
            {
                // Return what we got
            }
            return result;
        }

        // Helper method to convert JsonElement value
        private object ConvertJsonElement(object item)
        {
            try
            {
                if (item is System.Text.Json.JsonElement jsonElement)
                {
                    return jsonElement.GetRawText();
                }
                return item;
                        }
                        catch
                        {
                            return null;
                        }
                    }
                }
            }
