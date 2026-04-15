using Microsoft.AspNetCore.Mvc;
using WelfareLink.Services;

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
        return View(record);
    }

    // GET: ComplianceRecord/Create
    public async Task<IActionResult> Create(string? entityType, int? entityId, string? violationType, string? description)
    {
        var deny = Authorize();
        if (deny != null) return deny;

        ViewBag.EntityType = entityType;
        ViewBag.EntityId = entityId;

        // Pre-fill model values from query string if provided
        var model = new WelfareLink.Models.ComplainceRecord
        {
            EntityType = entityType ?? string.Empty,
            EntityId = entityId ?? 0,
            ViolationType = violationType ?? string.Empty,
            Description = description ?? string.Empty
        };

        // Prevent duplicate open compliance records for the same entity
        try
        {
            var open = await _api.GetOpenComplianceRecordsAsync();
            var existing = open.FirstOrDefault(r => r.EntityType == model.EntityType && r.EntityId == model.EntityId);
            if (existing != null)
            {
                ViewBag.AlreadyFlagged = true;
                ViewBag.ExistingRecordId = existing.RecordID;
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

        // If resolved or dismissed, return to the ComplianceOfficer dashboard so the
        // application flag state is refreshed immediately. Otherwise stay on Details.
        if (string.Equals(status, "Resolved", StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, "Dismissed", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction("Dashboard", "ComplianceOfficer");
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
