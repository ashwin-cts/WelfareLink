using Microsoft.AspNetCore.Mvc;
using WelfareLink.Services;

namespace WelfareLink.Controllers;

public class AuditLogController : Controller
{
    private readonly WelfareApiClient _api;
    private static readonly string[] _allowedRoles = ["Admin", "ComplianceOfficer", "GovernmentAuditor"];

    public AuditLogController(WelfareApiClient api) => _api = api;

    private IActionResult? Authorize()
    {
        var role = HttpContext.Session.GetString("UserRole");
        if (!_allowedRoles.Contains(role))
            return RedirectToAction("Login", "Account");
        return null;
    }

    // GET: AuditLog
    public async Task<IActionResult> Index()
    {
        var deny = Authorize();
        if (deny != null) return deny;

        var logs = await _api.GetAllAuditLogsAsync();
        return View(logs);
    }
}
