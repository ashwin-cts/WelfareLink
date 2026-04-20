using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using WelfareLink.Models;
using WelfareLink.Services;

namespace WelfareLink.Controllers
{
    public class AuditorController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly WelfareApiClient _apiClient;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public AuditorController(IHttpClientFactory httpClientFactory, WelfareApiClient apiClient)
        {
            _httpClientFactory = httpClientFactory;
            _apiClient = apiClient;
        }

        private async Task<List<T>> DeserializeResponse<T>(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
                return new List<T>();

            var json = await response.Content.ReadAsStringAsync();
            try
            {
                return JsonSerializer.Deserialize<List<T>>(json, _jsonOptions) ?? new List<T>();
            }
            catch
            {
                return new List<T>();
            }
        }

        private bool CheckAuthorization()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Auditor" && userRole != "GovernmentAuditor")
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Dashboard - Displays summary metrics
        /// Total Applications, Total Programs, Total Budget, Total Resource, Total Disbursement
        /// </summary>
        public async Task<IActionResult> Dashboard()
        {
            if (!CheckAuthorization())
                return RedirectToAction("Login", "Account");

            try
            {
                var client = _httpClientFactory.CreateClient("DashboardClient");

                dynamic dashboardData = new System.Dynamic.ExpandoObject();

                // Get applications
                var applicationsResponse = await client.GetAsync("api/welfareapplicationapi");
                var applications = await DeserializeResponse<WelfareApplication>(applicationsResponse);
                dashboardData.TotalApplications = applications.Count;

                // Get programs
                var programsResponse = await client.GetAsync("api/welfareprogramapi");
                var programs = await DeserializeResponse<WelfareProgram>(programsResponse);
                dashboardData.TotalPrograms = programs.Count;

                // Calculate total budget
                decimal totalBudget = programs.Sum(p => p.Budget);
                dashboardData.TotalBudget = totalBudget;

                // Get resources
                var resourcesResponse = await client.GetAsync("api/resourceapi");
                var resources = await DeserializeResponse<Resource>(resourcesResponse);
                decimal totalResource = resources.Sum(r => r.Quantity);
                dashboardData.TotalResource = totalResource;

                // Get disbursements
                var disbursementsResponse = await client.GetAsync("api/disbursementapi");
                var disbursements = await DeserializeResponse<Disbursement>(disbursementsResponse);
                decimal totalDisbursement = (decimal)disbursements.Sum(d => d.Amount);
                dashboardData.TotalDisbursement = totalDisbursement;

                return View(dashboardData);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error loading dashboard: {ex.Message}";
                return View();
            }
        }

        /// <summary>
        /// Budget Monitoring - Program Breakdown
        /// Shows all programs with budget details, resources, citizens, disbursements
        /// </summary>
        public async Task<IActionResult> BudgetMonitoring()
        {
            if (!CheckAuthorization())
                return RedirectToAction("Login", "Account");

            try
            {
                var client = _httpClientFactory.CreateClient("DashboardClient");

                var programsResponse = await client.GetAsync("api/welfareprogramapi");
                var applicationsResponse = await client.GetAsync("api/welfareapplicationapi");
                var benefitsResponse = await client.GetAsync("api/benefitapi");
                var disbursementsResponse = await client.GetAsync("api/disbursementapi");
                var resourcesResponse = await client.GetAsync("api/resourceapi");

                var programs = await DeserializeResponse<WelfareProgram>(programsResponse);
                var applications = await DeserializeResponse<WelfareApplication>(applicationsResponse);
                var benefits = await DeserializeResponse<Benefit>(benefitsResponse);
                var disbursements = await DeserializeResponse<Disbursement>(disbursementsResponse);
                var resources = await DeserializeResponse<Resource>(resourcesResponse);

                var programBreakdown = new List<dynamic>();

                foreach (var program in programs)
                {
                    // Count citizens applied for this program
                    var citizensForProgram = applications
                        .Where(a => a.ProgramID == program.ProgramID)
                        .Select(a => a.CitizenID)
                        .Distinct()
                        .Count();

                    // Calculate total resources allocated to this program
                    decimal totalResourceAllocated = resources
                        .Where(r => r.ProgramID == program.ProgramID)
                        .Sum(r => r.Quantity);

                    // Calculate total disbursed for this program
                    decimal totalDisbursed = 0;

                    var programApplications = applications.Where(a => a.ProgramID == program.ProgramID);

                    foreach (var app in programApplications)
                    {
                        var appBenefits = benefits.Where(b => b.ApplicationID == app.ApplicationID);

                        foreach (var benefit in appBenefits)
                        {
                            var benefitDisbursements = disbursements.Where(d => d.BenefitID == benefit.BenefitID);

                            foreach (var disburse in benefitDisbursements)
                            {
                                totalDisbursed += (decimal)disburse.Amount;
                            }
                        }
                    }

                    decimal remaining = program.Budget - totalResourceAllocated;
                    decimal utilizationPercent = program.Budget > 0 ? (totalResourceAllocated / program.Budget) * 100 : 0;

                    dynamic item = new System.Dynamic.ExpandoObject();
                    var dict = (IDictionary<string, object>)item;
                    dict["ProgramID"] = program.ProgramID;
                    dict["ProgramName"] = program.Title;
                    dict["ProgramStatus"] = program.Status;
                    dict["ProgramBudget"] = program.Budget;
                    dict["AllocatedResource"] = totalResourceAllocated;
                    dict["CitizensApplied"] = citizensForProgram;
                    dict["TotalDisbursed"] = totalDisbursed;
                    dict["RemainingResource"] = remaining;
                    dict["UtilizationPercent"] = Math.Round(utilizationPercent, 2);

                    programBreakdown.Add(item);
                }

                return View(programBreakdown);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error loading budget monitoring: {ex.Message}";
                return View(new List<dynamic>());
            }
        }

        /// <summary>
        /// Resource Allocation Statement
        /// Shows resource allocation history from Program Officer
        /// </summary>
        public async Task<IActionResult> ResourceStatement()
        {
            if (!CheckAuthorization())
                return RedirectToAction("Login", "Account");

            try
            {
                var client = _httpClientFactory.CreateClient("DashboardClient");

                var resourcesResponse = await client.GetAsync("api/resourceapi");
                var programsResponse = await client.GetAsync("api/welfareprogramapi");
                var applicationsResponse = await client.GetAsync("api/welfareapplicationapi");
                var benefitsResponse = await client.GetAsync("api/benefitapi");

                var resources = await DeserializeResponse<Resource>(resourcesResponse);
                var programs = await DeserializeResponse<WelfareProgram>(programsResponse);
                var applications = await DeserializeResponse<WelfareApplication>(applicationsResponse);
                var benefits = await DeserializeResponse<Benefit>(benefitsResponse);

                var resourceStatements = new List<dynamic>();

                foreach (var resource in resources)
                {
                    var program = programs.FirstOrDefault(p => p.ProgramID == resource.ProgramID);

                    // Calculate total resources allocated to this program
                    decimal totalResourcesAllocated = resources
                        .Where(r => r.ProgramID == resource.ProgramID)
                        .Sum(r => r.Quantity);

                    decimal programBudget = program?.Budget ?? 0;
                    decimal remainingAllocation = programBudget - totalResourcesAllocated;

                    dynamic item = new System.Dynamic.ExpandoObject();
                    var dict = (IDictionary<string, object>)item;
                    dict["ResourceID"] = resource.ResourceID;
                    dict["Date"] = DateTime.Now;
                    dict["ProgramName"] = program?.Title ?? "Unknown";
                    dict["AllocatedResource"] = resource.Quantity;
                    dict["RemainingAllocationPending"] = Math.Max(remainingAllocation, 0);

                    resourceStatements.Add(item);
                }

                return View(resourceStatements);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error loading resource statement: {ex.Message}";
                return View(new List<dynamic>());
            }
        }

        /// <summary>
        /// Disbursement Statement
        /// Shows disbursement history with filters for Date and Citizen ID
        /// </summary>
        public async Task<IActionResult> DisbursementStatement(
            DateTime? filterDate = null, 
            int? filterCitizenId = null)
        {
            if (!CheckAuthorization())
                return RedirectToAction("Login", "Account");

            try
            {
                var client = _httpClientFactory.CreateClient("DashboardClient");

                var applicationResponse = await client.GetAsync("api/welfareapplicationapi");
                var benefitResponse = await client.GetAsync("api/benefitapi");
                var disbursementResponse = await client.GetAsync("api/disbursementapi");
                var programResponse = await client.GetAsync("api/welfareprogramapi");

                var applications = await DeserializeResponse<WelfareApplication>(applicationResponse);
                var benefits = await DeserializeResponse<Benefit>(benefitResponse);
                var disbursements = await DeserializeResponse<Disbursement>(disbursementResponse);
                var programs = await DeserializeResponse<WelfareProgram>(programResponse);

                var disbursementStatements = new List<dynamic>();

                // Apply citizen filter
                var filteredApplications = applications.AsEnumerable();
                if (filterCitizenId.HasValue)
                {
                    filteredApplications = filteredApplications.Where(a => a.CitizenID == filterCitizenId.Value);
                }

                foreach (var app in filteredApplications)
                {
                    var appBenefits = benefits.Where(b => b.ApplicationID == app.ApplicationID);

                    foreach (var benefit in appBenefits)
                    {
                        var appDisbursements = disbursements.Where(d => d.BenefitID == benefit.BenefitID);

                        // Apply date filter if provided
                        if (filterDate.HasValue)
                        {
                            appDisbursements = appDisbursements.Where(d => d.Date.Date == filterDate.Value.Date);
                        }

                        var program = programs.FirstOrDefault(p => p.ProgramID == app.ProgramID);

                        // If no disbursements for this benefit, still show it
                        if (!appDisbursements.Any())
                        {
                            dynamic item = new System.Dynamic.ExpandoObject();
                            var dict = (IDictionary<string, object>)item;
                            dict["CitizenID"] = app.CitizenID;
                            dict["CitizenName"] = app.Citizen?.Name ?? "Unknown";
                            dict["MaxBenefit"] = program?.MaxBenefitPerCitizen ?? 0;
                            dict["BenefitAllocated"] = (decimal)benefit.Amount;
                            dict["Disbursed"] = 0m;
                            dict["RemainDisburse"] = (decimal)benefit.Amount;
                            dict["DisbursementPercent"] = 0m;
                            disbursementStatements.Add(item);
                        }
                        else
                        {
                            decimal totalDisbursedForBenefit = (decimal)appDisbursements.Sum(d => d.Amount);
                            decimal remaining = (decimal)benefit.Amount - totalDisbursedForBenefit;
                            decimal disbursementPercent = benefit.Amount > 0 ? (totalDisbursedForBenefit / (decimal)benefit.Amount) * 100 : 0;

                            dynamic item = new System.Dynamic.ExpandoObject();
                            var dict = (IDictionary<string, object>)item;
                            dict["CitizenID"] = app.CitizenID;
                            dict["CitizenName"] = app.Citizen?.Name ?? "Unknown";
                            dict["MaxBenefit"] = program?.MaxBenefitPerCitizen ?? 0;
                            dict["BenefitAllocated"] = (decimal)benefit.Amount;
                            dict["Disbursed"] = totalDisbursedForBenefit;
                            dict["RemainDisburse"] = Math.Max(remaining, 0);
                            dict["DisbursementPercent"] = Math.Round(disbursementPercent, 2);
                            disbursementStatements.Add(item);
                        }
                    }
                }

                ViewBag.FilterDate = filterDate;
                ViewBag.FilterCitizenId = filterCitizenId;
                return View(disbursementStatements);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error loading disbursement statement: {ex.Message}";
                return View(new List<dynamic>());
            }
        }
    }
}
