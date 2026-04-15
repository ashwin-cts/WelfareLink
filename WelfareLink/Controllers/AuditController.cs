using Microsoft.AspNetCore.Mvc;
using WelfareLink.Services;

namespace WelfareLink.Controllers;

public class AuditController : Controller
{
    private readonly WelfareApiClient _api;
    private static readonly string[] _allowedRoles = ["Admin", "GovernmentAuditor", "ComplianceOfficer"];

    public AuditController(WelfareApiClient api) => _api = api;

    private IActionResult? Authorize()
    {
        var role = HttpContext.Session.GetString("UserRole");
        if (!_allowedRoles.Contains(role))
            return RedirectToAction("Login", "Account");
        return null;
    }

    // GET: Audit/Dashboard — Government Auditor main landing page
    public async Task<IActionResult> Dashboard()
    {
        var deny = Authorize();
        if (deny != null) return deny;

        var summaries = await _api.GetGovernmentAuditorDashboardAsync();
        return View(summaries);
    }

    // GET: Audit — all formal audit records
    public async Task<IActionResult> Index()
    {
        var deny = Authorize();
        if (deny != null) return deny;

        var audits = await _api.GetAllAuditsAsync();
        return View(audits);
    }

    // GET: Audit/Create
    public IActionResult Create(int? programId)
    {
        var deny = Authorize();
        if (deny != null) return deny;

        ViewBag.ProgramId = programId;
        return View();
    }

    // POST: Audit/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(WelfareLink.Models.Audit audit)
    {
        var deny = Authorize();
        if (deny != null) return deny;

        var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
        var apiAudit = new WelfareLink.Services.Audit
        {
            ProgramID = audit.ProgramID,
            AuditedByUserId = userId,
            FindingType = audit.FindingType,
            Description = audit.Description,
            Status = "Open"
        };

        var (created, error) = await _api.CreateAuditAsync(apiAudit);
        if (created != null)
        {
            TempData["SuccessMessage"] = "Audit finding recorded successfully.";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError(string.Empty, error ?? "Failed to create audit.");
        return View(audit);
    }

    // POST: Audit/UpdateStatus/{id}
    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var deny = Authorize();
        if (deny != null) return deny;

        await _api.UpdateAuditStatusAsync(id, status);
        TempData["SuccessMessage"] = $"Audit #{id} status updated to {status}."; 
        return RedirectToAction(nameof(Index));
    }
}
