using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WelfareLink.Interfaces;
using WelfareLink.Models;
using WelfareLink.Services;

namespace WelfareLink.Controllers;

public class WelfareApplicationController : Controller
{
    private readonly IWelfareApplicationService _welfareApplicationService;
    private readonly IWelfareProgramService _welfareProgramService;

    public WelfareApplicationController(IWelfareApplicationService welfareApplicationService, IWelfareProgramService welfareProgramService)
    {
        _welfareApplicationService = welfareApplicationService;
        _welfareProgramService = welfareProgramService;
    }

    public async Task<IActionResult> HomeIndex()
    {
        var applications = await _welfareApplicationService.GetAllApplicationsAsync();
        var programs = await _welfareProgramService.GetAllProgramsAsync();
        ViewBag.ProgramList = new SelectList(programs, "ProgramID", "Title");
        return View(applications);
    }


    // GET: WelfareApplication
    // Application Review Dashboard (Officer) - Lists applications pending review
    public async Task<IActionResult> Index()
    {
        var applications = await _welfareApplicationService.GetAllApplicationsAsync();
        return View(applications);
    }
    // GET: WelfareApplication/Pending
    // Get all pending applications for officer review
    public async Task<IActionResult> Pending()
    {
        var pendingApplications = await _welfareApplicationService.GetPendingApplicationsAsync();
        return View(pendingApplications);
    }

    // GET: WelfareApplication/Details/5
    // Application Detail Page - Full view of a single application
    public async Task<IActionResult> Details(int id)
    {
        var application = await _welfareApplicationService.GetApplicationByIdAsync(id);
        if (application == null)
        {
            return NotFound();
        }
        return View(application);
    }
    // GET: WelfareApplication/Create
    // Application Submission Page - Display form
    public async Task<IActionResult> Create()
    {
        var programs = await _welfareProgramService.GetAllProgramsAsync();
        ViewBag.ProgramList = new SelectList(programs, "ProgramID", "Title");
        return View();
    }

    // POST: WelfareApplication/Create
    // Application Submission Page - Submit application
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(WelfareApplication application)
    {
        if (ModelState.IsValid)
        {
            await _welfareApplicationService.SubmitApplicationAsync(application);
            TempData["SuccessMessage"] = "Application submitted successfully!";
            return RedirectToAction(nameof(MyApplications));
        }
        var programs = await _welfareProgramService.GetAllProgramsAsync();
        ViewBag.ProgramList = new SelectList(programs, "ProgramID", "Title");
        return View(application);
    }
    // GET: WelfareApplication/MyApplications
    // Application Status Page (Citizen) - Track application status
    public async Task<IActionResult> MyApplications()
    {
        // TODO: Filter by logged-in citizen's ID once authentication is implemented
        var applications = await _welfareApplicationService.GetAllApplicationsAsync();
        return View(applications);
    }

    // GET: WelfareApplication/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var application = await _welfareApplicationService.GetApplicationByIdAsync(id);
        if (application == null)
        {
            return NotFound();
        }
        return View(application);
    }
    // POST: WelfareApplication/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, WelfareApplication application)
    {
        if (id != application.ApplicationID)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            await _welfareApplicationService.UpdateApplicationAsync(application);
            TempData["SuccessMessage"] = "Application updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        return View(application);
    }
    // POST: WelfareApplication/UpdateStatus/5
    // Update application status (Approved/Rejected/Under Review)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var result = await _welfareApplicationService.UpdateApplicationStatusAsync(id, status);
        if (result)
        {
            TempData["SuccessMessage"] = $"Application status updated to {status}";
            return RedirectToAction(nameof(Details), new { id });
        }
        TempData["ErrorMessage"] = "Failed to update application status";
        return RedirectToAction(nameof(Index));
    }
    // GET: WelfareApplication/ByStatus
    // Filter applications by status
    public async Task<IActionResult> ByStatus(string status)
    {
        if (string.IsNullOrEmpty(status))
        {
            return RedirectToAction(nameof(Index));
        }

        var applications = await _welfareApplicationService.GetApplicationsByStatusAsync(status);
        ViewBag.Status = status;
        return View("Index", applications);
    }
    // GET: WelfareApplication/DateRange
    // Filter applications by date range
    public async Task<IActionResult> DateRange(DateOnly? startDate, DateOnly? endDate)
    {
        if (!startDate.HasValue || !endDate.HasValue)
        {
            return RedirectToAction(nameof(Index));
        }

        var applications = await _welfareApplicationService.GetApplicationsByDateRangeAsync(startDate.Value, endDate.Value);
        ViewBag.StartDate = startDate.Value;
        ViewBag.EndDate = endDate.Value;
        return View("Index", applications);
    }
    // GET: WelfareApplication/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var application = await _welfareApplicationService.GetApplicationByIdAsync(id);
        if (application == null)
        {
            return NotFound();
        }
        return View(application);
    }
    // POST: WelfareApplication/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _welfareApplicationService.DeleteApplicationAsync(id);
        TempData["SuccessMessage"] = "Application deleted successfully!";
        return RedirectToAction(nameof(Index));
    }

}
