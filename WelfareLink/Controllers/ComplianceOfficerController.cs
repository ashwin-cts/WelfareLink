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

                // Configure JSON options for case-insensitive deserialization
                var jsonOptions = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
                };

                // Fetch application as strongly typed model with case-insensitive deserialization
                var response = await client.GetAsync($"api/welfareapplicationapi/{id}");
                if (!response.IsSuccessStatusCode) 
                {
                    ViewBag.Error = $"API returned status: {response.StatusCode}";
                    return NotFound();
                }

                var content = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"API Response: {content}");

                var application = System.Text.Json.JsonSerializer.Deserialize<WelfareLink.Models.WelfareApplication>(content, jsonOptions);
                if (application == null) 
                {
                    ViewBag.Error = "Failed to deserialize application data";
                    return NotFound();
                }

                // Debug: Check if Benefits were loaded
                System.Diagnostics.Debug.WriteLine($"Application ID: {application.ApplicationID}");
                System.Diagnostics.Debug.WriteLine($"Benefits Count: {application.Benefits?.Count ?? 0}");
                if (application.Benefits != null)
                {
                    foreach (var b in application.Benefits)
                    {
                        System.Diagnostics.Debug.WriteLine($"Benefit {b.BenefitID}: Type={b.Type}, Disbursements={b.Disbursements?.Count ?? 0}");
                    }
                }

                // Fetch program-level resources if program id exists
                WelfareLink.Models.ProgramResourcesDto resources = null;
                try
                {
                    var programId = application.ProgramID;
                    if (programId != 0)
                    {
                        var resourceResponse = await client.GetAsync($"api/resourceapi/program/{programId}");
                        if (resourceResponse.IsSuccessStatusCode)
                        {
                            var resourceContent = await resourceResponse.Content.ReadAsStringAsync();
                            resources = System.Text.Json.JsonSerializer.Deserialize<WelfareLink.Models.ProgramResourcesDto>(resourceContent, jsonOptions);
                        }
                    }
                }
                catch (Exception ex) 
                { 
                    System.Diagnostics.Debug.WriteLine($"Error fetching resources: {ex.Message}");
                }

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
                System.Diagnostics.Debug.WriteLine($"Exception: {ex}");
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
