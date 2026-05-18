using Microsoft.AspNetCore.Mvc;
using WelfareLink.Services;
using WelfareLink.Models;
using System.Text.Json;

namespace WelfareLink.Controllers;

public class ComplainceRecordController : Controller
{
    private readonly WelfareApiClient _api;
    private static readonly string[] _allowedRoles = ["Admin", "ComplianceOfficer"];

    public ComplainceRecordController(WelfareApiClient api) => _api = api;

    private IActionResult? Authorize()
    {
        var role = HttpContext.Session.GetString("UserRole");
        if (!_allowedRoles.Contains(role))
            return RedirectToAction("Login", "Account");
        return null;
    }

    // GET: ComplianceRecord — Dashboard / all records
    public async Task<IActionResult> Index(string? status)
    {
        var deny = Authorize();
        if (deny != null) return deny;

        var records = await _api.GetAllComplianceRecordsAsync();
        if (!string.IsNullOrEmpty(status))
            records = records.Where(r => r.Status == status);

        ViewBag.StatusFilter = status;
        ViewBag.OpenCount = records.Count(r => r.Status == "Open");
        ViewBag.InvestigatingCount = records.Count(r => r.Status == "Under Investigation");
        ViewBag.ResolvedCount = records.Count(r => r.Status == "Resolved");
        ViewBag.DismissedCount = records.Count(r => r.Status == "Dismissed");
        return View(records);
    }

    // GET: ComplianceRecord/Details/{id}
    public async Task<IActionResult> Details(int id)
    {
        var deny = Authorize();
        if (deny != null) return deny;

        var record = await _api.GetComplianceRecordByIdAsync(id);
        if (record == null) return NotFound();

        // Load application details based on entity type
        WelfareApplication? application = null;
        ProgramResourcesDto? resources = null;
        int applicationId = 0;

        try
        {
            var jsonOptions = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            };

            var client = new HttpClient();
            var apiBaseUrl = HttpContext.RequestServices.GetRequiredService<IConfiguration>()["ApiSettings:BaseUrl"];
            client.BaseAddress = new Uri(apiBaseUrl!);

            // Determine ApplicationID based on entity type
            if (record.EntityType?.Equals("Application", StringComparison.OrdinalIgnoreCase) == true)
            {
                applicationId = record.EntityId;
            }
            else if (record.EntityType?.Equals("Benefit", StringComparison.OrdinalIgnoreCase) == true)
            {
                // Fetch benefit to get ApplicationID
                var benefitResponse = await client.GetAsync($"api/benefitapi/{record.EntityId}");
                if (benefitResponse.IsSuccessStatusCode)
                {
                    var benefitContent = await benefitResponse.Content.ReadAsStringAsync();
                    var benefit = JsonSerializer.Deserialize<Benefit>(benefitContent, jsonOptions);
                    if (benefit != null)
                    {
                        applicationId = benefit.ApplicationID;
                    }
                }
            }
            else if (record.EntityType?.Equals("Disbursement", StringComparison.OrdinalIgnoreCase) == true)
            {
                // Fetch disbursement to get BenefitID, then fetch benefit to get ApplicationID
                var disbursementResponse = await client.GetAsync($"api/disbursementapi/{record.EntityId}");
                if (disbursementResponse.IsSuccessStatusCode)
                {
                    var disbursementContent = await disbursementResponse.Content.ReadAsStringAsync();
                    var disbursement = JsonSerializer.Deserialize<Disbursement>(disbursementContent, jsonOptions);
                    if (disbursement != null)
                    {
                        var benefitResponse = await client.GetAsync($"api/benefitapi/{disbursement.BenefitID}");
                        if (benefitResponse.IsSuccessStatusCode)
                        {
                            var benefitContent = await benefitResponse.Content.ReadAsStringAsync();
                            var benefit = JsonSerializer.Deserialize<Benefit>(benefitContent, jsonOptions);
                            if (benefit != null)
                            {
                                applicationId = benefit.ApplicationID;
                            }
                        }
                    }
                }
            }

            // Fetch application details if we have an applicationId
            if (applicationId > 0)
            {
                var appResponse = await client.GetAsync($"api/welfareapplicationapi/{applicationId}");
                if (appResponse.IsSuccessStatusCode)
                {
                    var appContent = await appResponse.Content.ReadAsStringAsync();
                    application = JsonSerializer.Deserialize<WelfareApplication>(appContent, jsonOptions);

                    // Fetch program resources if application exists
                    if (application?.ProgramID > 0)
                    {
                        var resourceResponse = await client.GetAsync($"api/resourceapi/program/{application.ProgramID}");
                        if (resourceResponse.IsSuccessStatusCode)
                        {
                            var resourceContent = await resourceResponse.Content.ReadAsStringAsync();
                            resources = JsonSerializer.Deserialize<ProgramResourcesDto>(resourceContent, jsonOptions);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log but don't fail - compliance record details should still show
            System.Diagnostics.Debug.WriteLine($"Error loading application details: {ex.Message}");
        }

        // Create view model with compliance record and application details
        var vm = new ComplainceRecordDetailsViewModel
        {
            ComplianceRecord = record,
            Application = application,
            ProgramResources = resources
        };

        return View(vm);
    }

    // GET: ComplianceRecord/Create
    public async Task<IActionResult> Create(int? applicationId)
    {
        var deny = Authorize();
        if (deny != null) return deny;

        // Compliance violations can ONLY be created from ApplicationDetails with a valid applicationId
        if (!applicationId.HasValue || applicationId.Value <= 0)
        {
            TempData["ErrorMessage"] = "Application ID is required to raise a compliance violation. Please access this form from the Application Details page.";
            return RedirectToAction("Dashboard", "ComplianceOfficer");
        }

        // Initialize model - can only be created from ApplicationDetails with applicationId
        var model = new WelfareLink.Models.ComplainceRecord
        {
            ApplicationId = applicationId.Value
        };

        // Load application data for cascading form
        WelfareApplication? application = null;
        try
        {
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            };

            var client = new HttpClient();
            var apiBaseUrl = HttpContext.RequestServices.GetRequiredService<IConfiguration>()["ApiSettings:BaseUrl"];
            client.BaseAddress = new Uri(apiBaseUrl!);

            // Fetch application with all details
            var response = await client.GetAsync($"api/welfareapplicationapi/{applicationId.Value}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                application = JsonSerializer.Deserialize<WelfareApplication>(content, jsonOptions);
            }

            // If application doesn't exist, show error
            if (application == null)
            {
                TempData["ErrorMessage"] = $"Application #{applicationId.Value} not found. Please verify the application ID and try again.";
                return RedirectToAction("Dashboard", "ComplianceOfficer");
            }

            // Set application context
            model.ApplicationId = application.ApplicationID;
            model.ApplicationName = $"Application #{application.ApplicationID}";
            model.CitizenName = application.Citizen?.Name ?? "Unknown";

            // Build Benefits list
            if (application.Benefits != null)
            {
                model.BenefitsList = application.Benefits
                    .Select(b => new BenefitOption
                    {
                        BenefitID = b.BenefitID,
                        Type = b.Type,
                        Amount = b.Amount,
                        Status = b.Status
                    })
                    .ToList();

                // Build Disbursements list from all benefits
                foreach (var benefit in application.Benefits)
                {
                    if (benefit.Disbursements != null)
                    {
                        model.DisbursementsList.AddRange(
                            benefit.Disbursements.Select(d => new DisbursementOption
                            {
                                DisbursementID = d.DisbursementID,
                                BenefitID = d.BenefitID,
                                Amount = d.Amount,
                                Status = d.Status
                            })
                        );
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading application for compliance form: {ex.Message}");
            TempData["ErrorMessage"] = "An error occurred while loading the application details. Please try again.";
            return RedirectToAction("Dashboard", "ComplianceOfficer");
        }

        // Prevent duplicate open compliance records for the same entity
        try
        {
            if (!string.IsNullOrEmpty(model.EntityType) && model.EntityId > 0)
            {
                var open = await _api.GetOpenComplianceRecordsAsync();
                var existing = open.FirstOrDefault(r => r.EntityType == model.EntityType && r.EntityId == model.EntityId);
                if (existing != null)
                {
                    ViewBag.AlreadyFlagged = true;
                    ViewBag.ExistingRecordId = existing.RecordID;
                }
            }
        }
        catch
        {
            // swallow - non-critical for UX
        }

        return View(model);
    }

    // POST: ComplianceRecord/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(WelfareLink.Models.ComplainceRecord model)
    {
        var deny = Authorize();
        if (deny != null) return deny;
        // Server-side duplicate prevention: check for existing open record
        var userId = HttpContext.Session.GetInt32("UserId");
        try
        {
            var open = await _api.GetOpenComplianceRecordsAsync();
            var existing = open.FirstOrDefault(r => r.EntityType == model.EntityType && r.EntityId == model.EntityId);
            if (existing != null)
            {
                ModelState.AddModelError(string.Empty, "This entity already has an open compliance record.");
                ViewBag.AlreadyFlagged = true;
                ViewBag.ExistingRecordId = existing.RecordID;
                return View(model);
            }
        }
        catch
        {
            // ignore errors here and proceed to attempt creation; server-side API will also validate
        }

        var apiRecord = new WelfareLink.Services.ComplianceRecord
        {
            RaisedByUserId = userId,
            EntityType = model.EntityType,
            EntityId = model.EntityId,
            ViolationType = model.ViolationType,
            Description = model.Description,
            Status = "Open"
        };

        var (created, error) = await _api.CreateComplianceRecordAsync(apiRecord);
        if (created != null)
        {
            TempData["SuccessMessage"] = $"Compliance record #{created.RecordID} raised successfully.";
            // After creating a compliance record from the dashboard link, return user to the
            // ComplianceOfficer dashboard so the flagged state is visible immediately.
            return RedirectToAction("Dashboard", "ComplianceOfficer");
        }
        ModelState.AddModelError(string.Empty, error ?? "Failed to create record.");
        return View(model);
    }

    // POST: ComplianceRecord/UpdateStatus
    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int id, string status, string? notes)
    {
        var deny = Authorize();
        if (deny != null) return deny;

        var userId = HttpContext.Session.GetInt32("UserId");
        await _api.UpdateComplianceStatusAsync(id, status, userId, notes);
        TempData["SuccessMessage"] = $"Record #{id} status updated to {status}.";

        // Redirect to ComplainceRecord/Index after updating status
        return RedirectToAction(nameof(Index));
    }
}
