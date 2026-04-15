using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WelfareLink.Models;
using WelfareLink.Services;
using WelfareLink.ViewModels;

namespace WelfareLink.Controllers;

public class ResourceController : Controller
{
    private readonly WelfareApiClient _api;

    public ResourceController(WelfareApiClient api)
    {
        _api = api;
    }

    // GET: Resource
    public async Task<IActionResult> Index()
    {
        var resources = await _api.GetAllResourcesAsync();
        var programs = await _api.GetAllProgramsAsync();

        // Populate the Program navigation property for each resource
        var programDict = programs.ToDictionary(p => p.ProgramID);
        foreach (var resource in resources)
        {
            if (programDict.TryGetValue(resource.ProgramID, out var program))
            {
                resource.Program = program;
            }
        }

        return View(resources);
    }

    // GET: Resource/AllocateForm
    public async Task<IActionResult> AllocateForm(int? programId)
    {
        var programs = await _api.GetAllProgramsAsync();
        var activePrograms = programs.Where(p => p.Status == "Active");
        ViewBag.Programs = new SelectList(activePrograms, "ProgramID", "Title", programId);

        if (programId.HasValue)
        {
            var detail = await _api.GetResourcesByProgramIdAsync(programId.Value);
            if (detail != null)
            {
                ViewBag.ProgramTitle = detail.ProgramTitle;
                ViewBag.ProgramBudget = detail.ProgramBudget;
                ViewBag.AllocatedFunds = detail.TotalAllocated;
                ViewBag.RemainingBudget = detail.RemainingBudget;
            }
        }

        return View(new Resource { ProgramID = programId ?? 0 });
    }

    // POST: Resource/AllocateForm
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AllocateForm([Bind("ProgramID,Type,Quantity")] Resource resource)
    {
        ModelState.Remove("Status");
        ModelState.Remove("Program");

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "[400 Bad Request] Please fill all required fields correctly.";
            await ReloadAllocateFormData(resource.ProgramID);
            return View(resource);
        }

        var error = await _api.AddResourceAsync(resource);
        if (error == null)
        {
            TempData["SuccessMessage"] = "Resource allocated successfully!";
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = $"[400 Bad Request] {error}";
        await ReloadAllocateFormData(resource.ProgramID);
        return View(resource);
    }

    private async Task ReloadAllocateFormData(int programId)
    {
        var programs = await _api.GetAllProgramsAsync();
        ViewBag.Programs = new SelectList(programs.Where(p => p.Status == "Active"), "ProgramID", "Title", programId);

        if (programId > 0)
        {
            var detail = await _api.GetResourcesByProgramIdAsync(programId);
            if (detail != null)
            {
                ViewBag.ProgramTitle = detail.ProgramTitle;
                ViewBag.ProgramBudget = detail.ProgramBudget;
                ViewBag.AllocatedFunds = detail.TotalAllocated;
                ViewBag.RemainingBudget = detail.RemainingBudget;
            }
        }
    }

    // GET: Resource/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            TempData["ErrorMessage"] = "[400 Bad Request] Resource ID is required.";
            return RedirectToAction(nameof(Index));
        }

        var resources = await _api.GetAllResourcesAsync();
        var resource = resources.FirstOrDefault(r => r.ResourceID == id);
        if (resource == null)
        {
            TempData["ErrorMessage"] = "[404 Not Found] Resource not found.";
            return RedirectToAction(nameof(Index));
        }
        return View(resource);
    }

    // GET: Resource/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            TempData["ErrorMessage"] = "[400 Bad Request] Resource ID is required.";
            return RedirectToAction(nameof(Index));
        }

        var resources = await _api.GetAllResourcesAsync();
        var resource = resources.FirstOrDefault(r => r.ResourceID == id);
        if (resource == null)
        {
            TempData["ErrorMessage"] = "[404 Not Found] Resource not found.";
            return RedirectToAction(nameof(Index));
        }

        var programs = await _api.GetAllProgramsAsync();
        ViewBag.Programs = new SelectList(programs, "ProgramID", "Title", resource.ProgramID);
        ViewBag.ResourceTypes = new SelectList(new[] { "Funds", "Materials" }, resource.Type);
        ViewBag.ResourceStatuses = new SelectList(new[] { "Available", "Depleted", "Reserved" }, resource.Status);
        return View(resource);
    }

    // POST: Resource/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("ResourceID,ProgramID,Type,Quantity,Status")] Resource resource)
    {
        if (id != resource.ResourceID)
        {
            TempData["ErrorMessage"] = "[400 Bad Request] Invalid resource ID.";
            return RedirectToAction(nameof(Index));
        }

        ModelState.Remove("Program");
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "[400 Bad Request] Please fill all required fields correctly.";
            var programs = await _api.GetAllProgramsAsync();
            ViewBag.Programs = new SelectList(programs, "ProgramID", "Title", resource.ProgramID);
            ViewBag.ResourceTypes = new SelectList(new[] { "Funds", "Materials" }, resource.Type);
            ViewBag.ResourceStatuses = new SelectList(new[] { "Available", "Depleted", "Reserved" }, resource.Status);
            return View(resource);
        }

        var error = await _api.UpdateResourceAsync(resource);
        if (error == null)
        {
            TempData["SuccessMessage"] = "Resource updated successfully!";
            return RedirectToAction(nameof(ManageResources), new { programId = resource.ProgramID });
        }

        TempData["ErrorMessage"] = $"[400 Bad Request] {error}";
        var allPrograms = await _api.GetAllProgramsAsync();
        ViewBag.Programs = new SelectList(allPrograms, "ProgramID", "Title", resource.ProgramID);
        ViewBag.ResourceTypes = new SelectList(new[] { "Funds", "Materials" }, resource.Type);
        ViewBag.ResourceStatuses = new SelectList(new[] { "Available", "Depleted", "Reserved" }, resource.Status);
        return View(resource);
    }

    // GET: Resource/ManageResources/5
    public async Task<IActionResult> ManageResources(int? programId)
    {
        if (programId == null) return RedirectToAction("Index", "WelfareProgram");

        var detail = await _api.GetResourcesByProgramIdAsync(programId.Value);
        if (detail == null) return NotFound();

        ViewBag.ProgramTitle = detail.ProgramTitle;
        ViewBag.ProgramBudget = detail.ProgramBudget;
        ViewBag.TotalAllocated = detail.TotalAllocated;
        ViewBag.RemainingBudget = detail.RemainingBudget;
        ViewBag.UtilisationPercentage = detail.UtilisationPercentage;

        return View(detail.Resources);
    }

    // GET: Resource/UtilisationReport
    public async Task<IActionResult> UtilisationReport()
    {
        var utilisationViewModels = await _api.GetResourceUtilisationAsync();
        return View(utilisationViewModels);
    }
}
