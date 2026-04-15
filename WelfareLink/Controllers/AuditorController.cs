using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace WelfareLink.Controllers
{
    public class AuditorController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AuditorController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private bool CheckAuthorization()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "GovernmentAuditor")
            {
                return false;
            }
            return true;
        }

        public async Task<IActionResult> Dashboard()
        {
            if (!CheckAuthorization())
                return RedirectToAction("Login", "Account");

            try
            {
                var client = _httpClientFactory.CreateClient("DashboardClient");
                var stats = await client.GetFromJsonAsync<dynamic>("api/AuditorDashboard/statistics");
                var budgetStatus = await client.GetFromJsonAsync<dynamic>("api/AuditorDashboard/budget-status");
                var flaggedBenefits = await client.GetFromJsonAsync<dynamic>("api/AuditorDashboard/flagged-benefits");

                ViewBag.StatsJson = stats;
                ViewBag.BudgetStatusJson = budgetStatus;
                ViewBag.FlaggedBenefitsJson = flaggedBenefits;

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error loading dashboard: {ex.Message}";
                return View();
            }
        }

        public async Task<IActionResult> BudgetMonitoring()
        {
            if (!CheckAuthorization())
                return RedirectToAction("Login", "Account");

            try
            {
                var client = _httpClientFactory.CreateClient("DashboardClient");
                var budgetData = await client.GetFromJsonAsync<dynamic>("api/AuditorDashboard/budget-monitoring");
                var allocation = await client.GetFromJsonAsync<dynamic>("api/AuditorDashboard/resource-allocation");

                ViewBag.BudgetJson = budgetData;
                ViewBag.AllocationJson = allocation;
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error loading budget data: {ex.Message}";
                return View();
            }
        }

        public async Task<IActionResult> SystemLogs(DateTime? from, DateTime? to)
        {
            if (!CheckAuthorization())
                return RedirectToAction("Login", "Account");

            try
            {
                var client = _httpClientFactory.CreateClient("DashboardClient");
                var query = "api/AuditorDashboard/system-logs";
                if (from.HasValue && to.HasValue)
                {
                    query += $"?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}";
                }

                var logs = await client.GetFromJsonAsync<dynamic>(query);
                ViewBag.LogsJson = logs;
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error loading system logs: {ex.Message}";
                return View();
            }
        }
    }
}
