using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WelfareLink.Models;
using WelfareLink.Services;

namespace WelfareLink.Controllers
{
    public class BenefitController : Controller
    {
        private readonly WelfareApiClient _api;

        public BenefitController(WelfareApiClient api)
        {
            _api = api;
        }

        private async Task PopulateApplicationDropdown(int? selectedId = null)
        {
            var data = await _api.GetBenefitDropdownAsync(selectedId);

            ViewBag.ApplicationList = new SelectList(
                data?.Dropdown.Select(d => new { d.ApplicationID, d.Display }) ?? [],
                "ApplicationID", "Display", selectedId);

            ViewBag.ApplicationsJson = System.Text.Json.JsonSerializer.Serialize(data?.Applications ?? []);
        }

        // GET: Benefit
        public async Task<IActionResult> Index()
        {
            var benefits = await _api.GetAllBenefitsAsync();
            return View(benefits);
        }

        // GET: Benefit/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var benefit = await _api.GetBenefitByIdAsync(id.Value);
            if (benefit == null) return NotFound();

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
                var (created, error) = await _api.CreateBenefitAsync(benefit, HttpContext.Session.GetInt32("UserId") ?? 0);
                if (created != null)
                {
                    if (created.Status.Equals("Allocated", StringComparison.OrdinalIgnoreCase))
                    {
                        TempData["SuccessMessage"] = $"Benefit #{created.BenefitID} has been successfully allocated. " +
                            "A disbursement entry has been created \u2014 please process it below.";
                        return RedirectToAction("Index", "Disbursement");
                    }
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(string.Empty, error ?? "Failed to create benefit.");
            }
            await PopulateApplicationDropdown(benefit.ApplicationID);
            return View(benefit);
        }

        // GET: Benefit/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var benefit = await _api.GetBenefitByIdAsync(id.Value);
            if (benefit == null) return NotFound();

            await PopulateApplicationDropdown(benefit.ApplicationID);
            return View(benefit);
        }

        // POST: Benefit/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BenefitID,ApplicationID,Type,Amount,Date,Status")] Benefit benefit)
        {
            if (id != benefit.BenefitID) return NotFound();

            if (ModelState.IsValid)
            {
                if (!await _api.BenefitExistsAsync(benefit.BenefitID)) return NotFound();

                var (updated, error) = await _api.UpdateBenefitAsync(benefit, HttpContext.Session.GetInt32("UserId") ?? 0);
                if (updated != null)
                {
                    if (updated.Status.Equals("Allocated", StringComparison.OrdinalIgnoreCase))
                    {
                        TempData["SuccessMessage"] = $"Benefit #{updated.BenefitID} has been allocated successfully. " +
                            "Please process the disbursement entry below.";
                        return RedirectToAction("Index", "Disbursement");
                    }
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(string.Empty, error ?? "Failed to update benefit.");
            }
            await PopulateApplicationDropdown(benefit.ApplicationID);
            return View(benefit);
        }

        // GET: Benefit/GetProgramResourceInfo?programId=5
        [HttpGet]
        public async Task<IActionResult> GetProgramResourceInfo(int programId)
        {
            if (programId <= 0) return Json(null);
            var info = await _api.GetProgramResourceInfoAsync(programId);
            return Json(info);
        }

        // GET: Benefit/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var benefit = await _api.GetBenefitByIdAsync(id.Value);
            if (benefit == null) return NotFound();

            return View(benefit);
        }

        // POST: Benefit/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _api.DeleteBenefitAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
