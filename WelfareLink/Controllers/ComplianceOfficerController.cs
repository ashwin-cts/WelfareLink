using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace WelfareLink.Controllers
{
    public class ComplianceOfficerController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ComplianceOfficerController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private bool CheckAuthorization()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "ComplianceOfficer")
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
                var allocations = await client.GetFromJsonAsync<dynamic>("api/complianceofficerdashboardapi/allocations");
                var issues = await client.GetFromJsonAsync<dynamic>("api/complianceofficerdashboardapi/issues");
                var metrics = await client.GetFromJsonAsync<dynamic>("api/complianceofficerdashboardapi/metrics");

                ViewBag.AllocationsJson = allocations;
                ViewBag.IssuesJson = issues;
                ViewBag.StatsJson = metrics;

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error loading dashboard: {ex.Message}";
                return View();
            }
        }

        // GET: ComplianceOfficer/ApplicationDetails/5
        public async Task<IActionResult> ApplicationDetails(int id)
        {
            if (!CheckAuthorization())
                return RedirectToAction("Login", "Account");

            try
            {
                var client = _httpClientFactory.CreateClient("DashboardClient");
                // Fetch application as strongly typed model to avoid dynamic binder issues
                var application = await client.GetFromJsonAsync<WelfareLink.Models.WelfareApplication>($"api/welfareapplicationapi/{id}");
                if (application == null) return NotFound();

                // Fetch program-level resources if program id exists
                WelfareLink.Models.ProgramResourcesDto resources = null;
                try
                {
                    var programId = application.ProgramID;
                    if (programId != 0)
                    {
                        resources = await client.GetFromJsonAsync<WelfareLink.Models.ProgramResourcesDto>($"api/resourceapi/program/{programId}");
                    }
                }
                catch { /* ignore resource errors */ }

                // Build and pass a view model instead of a raw application model so the Razor
                // strongly-typed view receives the expected shape.
                var vm = new WelfareLink.Models.ApplicationDetailsViewModel
                {
                    Application = application,
                    ProgramResources = resources
                };
                ViewBag.ProgramResources = resources; // keep for backward compatibility if any views use it
                return View(vm);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error loading application details: {ex.Message}";
                return View();
            }
        }

        public async Task<IActionResult> MyAllocations()
        {
            if (!CheckAuthorization())
                return RedirectToAction("Login", "Account");

            try
            {
                var client = _httpClientFactory.CreateClient("DashboardClient");
                var allocations = await client.GetFromJsonAsync<dynamic>("api/complianceofficerdashboardapi/allocations");
                ViewBag.AllocationsJson = allocations;
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error loading allocations: {ex.Message}";
                return View();
            }
        }

        public async Task<IActionResult> MyIssues()
        {
            if (!CheckAuthorization())
                return RedirectToAction("Login", "Account");

            try
            {
                var client = _httpClientFactory.CreateClient("DashboardClient");
                var issues = await client.GetFromJsonAsync<dynamic>("api/complianceofficerdashboardapi/issues");
                ViewBag.IssuesJson = issues;
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error loading issues: {ex.Message}";
                return View();
            }
        }

        public async Task<IActionResult> RaiseCompliance(int benefitId)
        {
            if (!CheckAuthorization())
                return RedirectToAction("Login", "Account");

            ViewBag.BenefitId = benefitId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RaiseCompliance(int benefitId, string issue)
        {
            if (!CheckAuthorization())
                return RedirectToAction("Login", "Account");

            try
            {
                var client = _httpClientFactory.CreateClient("DashboardClient");
                var payload = new { benefitId, issue };
                var response = await client.PostAsJsonAsync("api/ComplianceOfficerDashboard/raise-issue", payload);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Compliance issue raised successfully";
                    return RedirectToAction("MyIssues");
                }
                else
                {
                    TempData["Error"] = "Error raising issue";
                    return RedirectToAction("MyIssues");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error raising issue: {ex.Message}";
                return RedirectToAction("MyIssues");
            }
        }
    }
}
