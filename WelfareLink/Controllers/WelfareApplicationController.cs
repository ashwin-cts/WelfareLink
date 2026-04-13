using Microsoft.AspNetCore.Mvc;
using WelfareLink.Models;
using WelfareLink.Services;

namespace WelfareLink.Controllers;

public class WelfareApplicationController : Controller
{
    private readonly WelfareApiClient _api;

    public WelfareApplicationController(WelfareApiClient api)
    {
        _api = api;
    }

    public async Task<IActionResult> HomeIndex(string? status = null)
    {
        var applications = await _api.GetAllApplicationsAsync(status);
        if (!string.IsNullOrEmpty(status)) ViewBag.CurrentStatus = status;
        return View(applications);
    }

    // GET: WelfareApplication
    public async Task<IActionResult> Index()
    {
        var applications = await _api.GetAllApplicationsAsync();
        return View(applications);
    }

    // GET: WelfareApplication/Pending
    public async Task<IActionResult> Pending()
    {
        var pendingApplications = await _api.GetPendingApplicationsAsync();
        return View(pendingApplications);
    }

    // GET: WelfareApplication/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var application = await _api.GetApplicationByIdAsync(id);
        if (application == null) return NotFound();
        return View(application);
    }

    // GET: WelfareApplication/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var application = await _api.GetApplicationByIdAsync(id);
        if (application == null) return NotFound();
        return View(application);
    }

    // POST: WelfareApplication/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, WelfareApplication application)
    {
        if (id != application.ApplicationID) return NotFound();

        if (ModelState.IsValid)
        {
            await _api.UpdateApplicationAsync(application);
            TempData["SuccessMessage"] = "Application updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        return View(application);
    }

    // POST: WelfareApplication/UpdateStatus/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var result = await _api.UpdateApplicationStatusAsync(id, status);
        if (result)
        {
            TempData["SuccessMessage"] = $"Application status updated to {status}";
            return RedirectToAction(nameof(Details), new { id });
        }
        TempData["ErrorMessage"] = "Failed to update application status";
        return RedirectToAction(nameof(Index));
    }

    // GET: WelfareApplication/ByStatus
    public async Task<IActionResult> ByStatus(string? status)
    {
        var applications = string.IsNullOrEmpty(status)
            ? await _api.GetAllApplicationsAsync()
            : await _api.GetAllApplicationsAsync(status);
        ViewBag.SelectedStatus = status ?? "";
        return View("Index", applications);
    }

    // GET: WelfareApplication/DateRange
    public async Task<IActionResult> DateRange(DateOnly? startDate, DateOnly? endDate)
    {
        if (!startDate.HasValue || !endDate.HasValue) return RedirectToAction(nameof(Index));

        var applications = await _api.GetAllApplicationsAsync();
        applications = applications.Where(a => a.SubmittedDate >= startDate.Value && a.SubmittedDate <= endDate.Value);
        ViewBag.StartDate = startDate.Value;
        ViewBag.EndDate = endDate.Value;
        return View("Index", applications);
    }

    // GET: WelfareApplication/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var application = await _api.GetApplicationByIdAsync(id);
        if (application == null) return NotFound();
        return View(application);
    }

    // POST: WelfareApplication/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _api.DeleteApplicationAsync(id);
        TempData["SuccessMessage"] = "Application deleted successfully!";
        return RedirectToAction(nameof(Index));
    }
}
