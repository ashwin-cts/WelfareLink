using Microsoft.AspNetCore.Mvc;
using WelfareLink.Models;
using WelfareLink.Services;

namespace WelfareLink.Controllers;

public class EligibilityCheckController : Controller
{
    private readonly WelfareApiClient _api;

    public EligibilityCheckController(WelfareApiClient api)
    {
        _api = api;
    }

    // GET: EligibilityCheck
    public async Task<IActionResult> Index()
    {
        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "WelfareOfficer" && userRole != "Admin")
            return RedirectToAction("Login", "Account");

        var checks = await _api.GetAllChecksAsync();
        return View(checks);
    }

    // GET: EligibilityCheck/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "WelfareOfficer" && userRole != "Admin")
            return RedirectToAction("Login", "Account");

        var check = await _api.GetCheckByIdAsync(id);
        if (check == null) return NotFound();
        return View(check);
    }

    // GET: EligibilityCheck/Create?applicationId=5
    public async Task<IActionResult> Create(int? applicationId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");

        if (userId == null || (userRole != "WelfareOfficer" && userRole != "Admin"))
            return RedirectToAction("Login", "Account");

        if (!applicationId.HasValue)
        {
            TempData["ErrorMessage"] = "Application ID is required. Please navigate from the Application Details page.";
            return RedirectToAction("Index", "WelfareApplication");
        }

        var info = await _api.GetEligibilityApplicationInfoAsync(applicationId.Value);
        if (info?.Application == null) return NotFound();

        ViewBag.Application = info.Application;
        ViewBag.Citizen = info.Citizen;
        ViewBag.Documents = info.Documents;
        ViewBag.HasApplicationDocs = info.Documents.Any();
        ViewBag.OfficerId = userId.Value;

        return View();
    }

    // POST: EligibilityCheck/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EligibilityCheck check, int? applicationId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");

        if (userId == null || (userRole != "WelfareOfficer" && userRole != "Admin"))
            return RedirectToAction("Login", "Account");

        if (!applicationId.HasValue)
        {
            TempData["ErrorMessage"] = "Application ID is required.";
            return RedirectToAction("Index", "WelfareApplication");
        }

        if (ModelState.IsValid)
        {
            await _api.CreateCheckAsync(check, applicationId);

            if (check.Result?.ToLower() == "eligible")
            {
                TempData["SuccessMessage"] = "Application is Eligible â€” status set to Approved and a Benefit has been automatically created.";
                return RedirectToAction("Index", "Benefit");
            }

            TempData["SuccessMessage"] = "Eligibility check recorded. Application has been marked as Rejected.";
            return applicationId.HasValue
                ? RedirectToAction("Details", "WelfareApplication", new { id = applicationId })
                : RedirectToAction(nameof(Index));
        }

        if (applicationId.HasValue)
        {
            var info = await _api.GetEligibilityApplicationInfoAsync(applicationId.Value);
            if (info != null)
            {
                ViewBag.Application = info.Application;
                ViewBag.Citizen = info.Citizen;
                ViewBag.Documents = info.Documents;
            }
            ViewBag.OfficerId = userId.Value;
        }
        return View(check);
    }

    // POST: EligibilityCheck/UpdateDocumentStatus (AJAX)
    [HttpPost]
    public async Task<IActionResult> UpdateDocumentStatus(int documentId, string status)
    {
        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "WelfareOfficer" && userRole != "Admin")
            return Json(new { success = false, message = "Unauthorized." });

        var allowed = new[] { "Approved", "Rejected" };
        if (!allowed.Contains(status))
            return Json(new { success = false, message = "Invalid status value." });

        var success = await _api.UpdateDocumentVerificationStatusAsync(documentId, status);
        if (success)
            return Json(new { success = true });

        return Json(new { success = false, message = "Failed to update document status." });
    }

    // GET: EligibilityCheck/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var check = await _api.GetCheckByIdAsync(id);
        if (check == null) return NotFound();
        return View(check);
    }

    // POST: EligibilityCheck/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EligibilityCheck check)
    {
        if (id != check.CheckID) return NotFound();

        if (ModelState.IsValid)
        {
            await _api.UpdateCheckAsync(check);
            TempData["SuccessMessage"] = "Eligibility check updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        return View(check);
    }
}
