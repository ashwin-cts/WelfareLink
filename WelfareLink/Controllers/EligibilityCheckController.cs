using Microsoft.AspNetCore.Mvc;
using WelfareLink.Interfaces;
using WelfareLink.Models;

namespace WelfareLink.Controllers;

public class EligibilityCheckController : Controller
{
    private readonly IEligibilityCheckService _eligibilityCheckService;
    private readonly IWelfareApplicationService _applicationService;

    public EligibilityCheckController(
        IEligibilityCheckService eligibilityCheckService, IWelfareApplicationService applicationService)
    {
        _eligibilityCheckService = eligibilityCheckService;
        _applicationService = applicationService;
    }

    // GET: EligibilityCheck
    // List all eligibility checks
    public async Task<IActionResult> Index()
    {
        var checks = await _eligibilityCheckService.GetAllChecksAsync();
        return View(checks);
    }

    // GET: EligibilityCheck/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var check = await _eligibilityCheckService.GetCheckByIdAsync(id);
        if (check == null)
        {
            return NotFound();
        }
        return View(check);
    }

    // GET: EligibilityCheck/Create
    // Eligibility Check Page (Officer) - Display form
    // Must be accessed from an application details page with applicationId parameter
    public async Task<IActionResult> Create(int? applicationId)
    {
        if (!applicationId.HasValue)
        {
            TempData["ErrorMessage"] = "Application ID is required. Please navigate from the Application Details page.";
            return RedirectToAction("Index", "WelfareApplication");
        }

        var application = await _applicationService.GetApplicationByIdAsync(applicationId.Value);
        if (application == null)
        {
            return NotFound();
        }

        ViewBag.Application = application;
        return View();
    }

    // POST: EligibilityCheck/Create
    // Eligibility Check Page (Officer) - Record eligibility assessment
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EligibilityCheck check, int? applicationId)
    {
        if (!applicationId.HasValue)
        {
            TempData["ErrorMessage"] = "Application ID is required.";
            return RedirectToAction("Index", "WelfareApplication");
        }
        if (ModelState.IsValid)
        {
            await _eligibilityCheckService.CreateCheckAsync(check, applicationId);

            if (check.Result?.ToLower() == "eligible")
            {
                TempData["SuccessMessage"] = "Application is Eligible — status set to Approved and a Benefit has been automatically created.";
                return RedirectToAction("Index", "Benefit");
            }

            TempData["SuccessMessage"] = "Eligibility check recorded. Application has been marked as Rejected.";
            if (applicationId.HasValue)
            {
                return RedirectToAction("Details", "WelfareApplication", new { id = applicationId });
            }
            return RedirectToAction(nameof(Index));
        }

        if (applicationId.HasValue)
        {
            var application = await _applicationService.GetApplicationByIdAsync(applicationId.Value);
            ViewBag.Application = application;
        }
        return View(check);
    }

    // GET: EligibilityCheck/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var check = await _eligibilityCheckService.GetCheckByIdAsync(id);
        if (check == null)
        {
            return NotFound();
        }
        return View(check);
    }

    // POST: EligibilityCheck/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EligibilityCheck check)
    {
        if (id != check.CheckID)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            await _eligibilityCheckService.UpdateCheckAsync(check);
            TempData["SuccessMessage"] = "Eligibility check updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        var existing = await _eligibilityCheckService.GetCheckByIdAsync(check.CheckID);
        if (existing != null) check.WelfareApplication = existing.WelfareApplication;
        return View(check);
    }

    // GET: EligibilityCheck/ByResult
    // Filter eligibility checks by result (Eligible/Ineligible)
    public async Task<IActionResult> ByResult(string result)
    {
        if (string.IsNullOrEmpty(result))
        {
            return RedirectToAction(nameof(Index));
        }

        var checks = await _eligibilityCheckService.GetChecksByResultAsync(result);
        ViewBag.Result = result;
        return View("Index", checks);
    }

    // GET: EligibilityCheck/DateRange
    // Filter eligibility checks by date range
    public async Task<IActionResult> DateRange(DateOnly? startDate, DateOnly? endDate)
    {
        if (!startDate.HasValue || !endDate.HasValue)
        {
            return RedirectToAction(nameof(Index));
        }

        var checks = await _eligibilityCheckService.GetChecksByDateRangeAsync(startDate.Value, endDate.Value);
        ViewBag.StartDate = startDate.Value;
        ViewBag.EndDate = endDate.Value;
        return View("Index", checks);
    }

    // GET: EligibilityCheck/ByApplication/5
    // Get all eligibility checks for a specific application
    public async Task<IActionResult> ByApplication(int applicationId)
    {
        var application = await _applicationService.GetApplicationByIdAsync(applicationId);
        if (application == null)
        {
            return NotFound();
        }

        var checks = await _eligibilityCheckService.GetChecksByApplicationIdAsync(applicationId);
        ViewBag.Application = application;
        return View("Index", checks);
    }

    // GET: EligibilityCheck/MyChecks
    // Get all eligibility checks performed by the logged-in officer
    public async Task<IActionResult> MyChecks()
    {
        // TODO: Get logged-in officer ID once authentication is implemented
        // For now, return all checks
        var checks = await _eligibilityCheckService.GetAllChecksAsync();
        return View("Index", checks);
    }

    // GET: EligibilityCheck/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var check = await _eligibilityCheckService.GetCheckByIdAsync(id);
        if (check == null)
        {
            return NotFound();
        }
        return View(check);
    }

    // POST: EligibilityCheck/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _eligibilityCheckService.DeleteCheckAsync(id);
        TempData["SuccessMessage"] = "Eligibility check deleted successfully!";
        return RedirectToAction(nameof(Index));
    }
}
