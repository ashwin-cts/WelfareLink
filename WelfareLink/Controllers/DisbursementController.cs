using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WelfareLink.Models;
using WelfareLink.Services;

namespace WelfareLink.Controllers
{
    public class DisbursementController : Controller
    {
        private readonly WelfareApiClient _api;

        public DisbursementController(WelfareApiClient api)
        {
            _api = api;
        }

        private async Task PopulateBenefitDropdown(int? selectedBenefitId = null)
        {
            var benefits = await _api.GetAllBenefitsAsync();
            var benefitList = benefits.Select(b => new SelectListItem
            {
                Value = b.BenefitID.ToString(),
                Text = $"{b.BenefitID} - {b.Type} - {b.Amount:C}",
                Selected = b.BenefitID == selectedBenefitId
            });
            ViewData["BenefitID"] = new SelectList(benefitList, "Value", "Text", selectedBenefitId);
        }

        // GET: Disbursement
        public async Task<IActionResult> Index()
        {
            var disbursements = await _api.GetAllDisbursementsAsync();
            return View(disbursements);
        }

        // GET: Disbursement/History
        public async Task<IActionResult> History(DateTime? startDate, DateTime? endDate, string? benefitType, int? officerId, string? status)
        {
            var disbursements = (await _api.GetAllDisbursementsAsync()).AsQueryable();
            var benefits = (await _api.GetAllBenefitsAsync()).ToDictionary(b => b.BenefitID);

            // Enrich disbursements with benefit information
            var enrichedDisbursements = disbursements.Select(d =>
            {
                if (benefits.TryGetValue(d.BenefitID, out var benefit))
                {
                    d.Benefit = benefit;
                }
                return d;
            }).AsQueryable();

            if (startDate.HasValue) enrichedDisbursements = enrichedDisbursements.Where(d => d.Date >= startDate.Value);
            if (endDate.HasValue) enrichedDisbursements = enrichedDisbursements.Where(d => d.Date <= endDate.Value);
            if (!string.IsNullOrEmpty(benefitType)) enrichedDisbursements = enrichedDisbursements.Where(d => d.Benefit != null && d.Benefit.Type == benefitType);
            if (officerId.HasValue) enrichedDisbursements = enrichedDisbursements.Where(d => d.OfficerID == officerId.Value);
            if (!string.IsNullOrEmpty(status)) enrichedDisbursements = enrichedDisbursements.Where(d => d.Status == status);

            ViewBag.BenefitTypes = benefits.Values.Select(b => b.Type).Distinct().ToList();
            ViewBag.Statuses = new List<string> { "Completed", "Pending", "Failed" };
            ViewBag.OfficerIds = enrichedDisbursements.Select(d => d.OfficerID).Distinct().OrderBy(o => o).ToList();
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.SelectedBenefitType = benefitType;
            ViewBag.SelectedOfficerId = officerId;
            ViewBag.SelectedStatus = status;

            return View(enrichedDisbursements.ToList());
        }

        // GET: Disbursement/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var detail = await _api.GetDisbursementByIdAsync(id.Value);
            if (detail?.Disbursement == null) return NotFound();

            ViewBag.SiblingDisbursements = detail.SiblingDisbursements;
            ViewBag.BenefitTotalAmount = detail.BenefitTotalAmount;
            ViewBag.TotalDisbursed = detail.TotalDisbursed;
            ViewBag.PendingBalance = detail.PendingBalance;

            return View(detail.Disbursement);
        }

        // GET: Disbursement/Create
        public async Task<IActionResult> Create()
        {
            var officerId = HttpContext.Session.GetInt32("UserId") ?? 0;
            await PopulateBenefitDropdown();
            return View(new Disbursement { OfficerID = officerId });
        }

        // POST: Disbursement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DisbursementID,BenefitID,CitizenID,OfficerID,Amount,Date,Status")] Disbursement disbursement)
        {
            var sessionOfficerId = HttpContext.Session.GetInt32("UserId");
            if (sessionOfficerId.HasValue) disbursement.OfficerID = sessionOfficerId.Value;

            if (ModelState.IsValid)
            {
                var (created, error) = await _api.CreateDisbursementAsync(disbursement);
                if (created != null) return RedirectToAction(nameof(Index));
                ModelState.AddModelError(string.Empty, error ?? "Failed to create disbursement.");
            }
            await PopulateBenefitDropdown(disbursement.BenefitID);
            ViewBag.OfficerId = disbursement.OfficerID;
            return View(disbursement);
        }

        // GET: Disbursement/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var detail = await _api.GetDisbursementByIdAsync(id.Value);
            if (detail?.Disbursement == null) return NotFound();

            if (detail.Disbursement.Status == "Completed")
            {
                TempData["Error"] = "Cannot edit disbursement";
                return RedirectToAction("Index");
            }
            await PopulateBenefitDropdown(detail.Disbursement.BenefitID);
            ViewBag.OfficerId = detail.Disbursement.OfficerID;
            return View(detail.Disbursement);
        }

        // POST: Disbursement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DisbursementID,BenefitID,CitizenID,OfficerID,Amount,Date,Status")] Disbursement disbursement)
        {
            if (id != disbursement.DisbursementID) return NotFound();

            if (ModelState.IsValid)
            {
                if (!await _api.DisbursementExistsAsync(disbursement.DisbursementID)) return NotFound();

                var (updated, error) = await _api.UpdateDisbursementAsync(disbursement);
                if (updated != null) return RedirectToAction(nameof(Index));
                ModelState.AddModelError(string.Empty, error ?? "Failed to update disbursement.");
            }
            await PopulateBenefitDropdown(disbursement.BenefitID);
            ViewBag.OfficerId = disbursement.OfficerID;
            return View(disbursement);
        }

        // GET: Disbursement/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var detail = await _api.GetDisbursementByIdAsync(id.Value);
            if (detail?.Disbursement == null) return NotFound();

            if (detail.Disbursement.Status == "Completed")
            {
                TempData["Error"] = "Cannot delete disbursement";
                return RedirectToAction("Index");
            }
            return View(detail.Disbursement);
        }

        // POST: Disbursement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _api.DeleteDisbursementAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // GET: Disbursement/GetBenefitDetails?benefitId=5
        [HttpGet]
        public async Task<IActionResult> GetBenefitDetails(int benefitId)
        {
            if (benefitId <= 0) return Json(null);
            var details = await _api.GetDisbursementBenefitDetailsAsync(benefitId);
            return Json(details);
        }
    }
}
