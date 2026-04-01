using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WelfareLink.Interfaces;
using WelfareLink.Models;

namespace WelfareLink.Controllers
{
    public class BenefitController : Controller
    {
        private readonly IBenefitService _benefitService;
        private readonly IWelfareApplicationService _welfareApplicationService;
        private readonly IResourceService _resourceService;

        public BenefitController(IBenefitService benefitService, IWelfareApplicationService welfareApplicationService, IResourceService resourceService)
        {
            _benefitService = benefitService;
            _welfareApplicationService = welfareApplicationService;
            _resourceService = resourceService;
        }

        private async Task PopulateApplicationDropdown(int? selectedId = null)
        {
            var applications = await _welfareApplicationService.GetAllApplicationsAsync();

            // Filter to show only APPROVED applications
            var appList = applications
                .Where(a => a.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                .ToList();

            ViewBag.ApplicationList = new SelectList(
                appList.Select(a => new {
                    a.ApplicationID,
                    Display = $"App #{a.ApplicationID} | {a.Citizen?.Name ?? $"Citizen #{a.CitizenID}"} | {a.Program?.Title ?? $"Program #{a.ProgramID}"}"
                }),
                "ApplicationID", "Display", selectedId);

            ViewBag.ApplicationsJson = System.Text.Json.JsonSerializer.Serialize(
                appList.Select(a => new {
                    a.ApplicationID,
                    a.CitizenID,
                    CitizenName     = a.Citizen?.Name ?? "-",
                    a.ProgramID,
                    ProgramTitle    = a.Program?.Title ?? $"Program #{a.ProgramID}",
                    ProgramDesc     = a.Program?.Description ?? "-",
                    ProgramBudget   = a.Program?.Budget,
                    ProgramStatus   = a.Program?.Status ?? "-",
                    SubmittedDate   = a.SubmittedDate.ToString("dd MMM yyyy"),
                    a.Status
                }));
        }

        // GET: Benefit
        public async Task<IActionResult> Index()
        {
            var benefits = await _benefitService.GetAllBenefitsAsync();
            return View(benefits);
        }

        // GET: Benefit/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var benefit = await _benefitService.GetBenefitByIdAsync(id.Value);
            if (benefit == null)
            {
                return NotFound();
            }

            return View(benefit);
        }

        // GET: Benefit/Create
        public async Task<IActionResult> Create()
        {
            await PopulateApplicationDropdown();
            return View();
        }

        // POST: Benefit/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BenefitID,ApplicationID,Type,Amount,Date,Status")] Benefit benefit)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var created = await _benefitService.CreateBenefitAsync(benefit, HttpContext.Session.GetInt32("UserId") ?? 0);
                    if (created.Status.Equals("Allocated", StringComparison.OrdinalIgnoreCase))
                    {
                        TempData["SuccessMessage"] = $"Benefit #{created.BenefitID} has been successfully allocated. " +
                            "A disbursement entry has been created \u2014 please process it below.";
                        return RedirectToAction("Index", "Disbursement");
                    }
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            await PopulateApplicationDropdown(benefit.ApplicationID);
            return View(benefit);
        }

        // GET: Benefit/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var benefit = await _benefitService.GetBenefitByIdAsync(id.Value);
            if (benefit == null)
            {
                return NotFound();
            }

            await PopulateApplicationDropdown(benefit.ApplicationID);
            return View(benefit);
        }

        // POST: Benefit/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BenefitID,ApplicationID,Type,Amount,Date,Status")] Benefit benefit)
        {
            if (id != benefit.BenefitID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (!await _benefitService.BenefitExistsAsync(benefit.BenefitID))
                {
                    return NotFound();
                }

                try
                {
                    var updated = await _benefitService.UpdateBenefitAsync(benefit, HttpContext.Session.GetInt32("UserId") ?? 0);
                    if (updated.Status.Equals("Allocated", StringComparison.OrdinalIgnoreCase))
                    {
                        TempData["SuccessMessage"] = $"Benefit #{updated.BenefitID} has been allocated successfully. " +
                            "Please process the disbursement entry below.";
                        return RedirectToAction("Index", "Disbursement");
                    }
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            await PopulateApplicationDropdown(benefit.ApplicationID);
            return View(benefit);
        }

        // GET: Benefit/GetProgramResourceInfo?programId=5
        [HttpGet]
        public async Task<IActionResult> GetProgramResourceInfo(int programId)
        {
            if (programId <= 0) return Json(null);

            var resources = await _resourceService.GetResourcesByProgramIdAsync(programId);
            var totalResource = (double)resources.Sum(r => r.Quantity);

            var allBenefits = await _benefitService.GetAllBenefitsAsync();
            var alreadyAllocated = allBenefits
                .Where(b => b.WelfareApplication?.ProgramID == programId
                            && !b.Status.Equals("Allocation Pending", StringComparison.OrdinalIgnoreCase)
                            && !b.Status.Equals("Failed", StringComparison.OrdinalIgnoreCase))
                .Sum(b => b.Amount);

            var remainingResource = totalResource - alreadyAllocated;

            return Json(new
            {
                totalResource,
                alreadyAllocated,
                remainingResource,
                hasResource = totalResource > 0
            });
        }

        // GET: Benefit/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var benefit = await _benefitService.GetBenefitByIdAsync(id.Value);
            if (benefit == null)
            {
                return NotFound();
            }

            return View(benefit);
        }

        // POST: Benefit/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _benefitService.DeleteBenefitAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
