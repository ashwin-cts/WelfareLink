using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WelfareLink.Interfaces;
using WelfareLink.Models;

namespace WelfareLinkPRJ.Controllers
{
    public class DisbursementController : Controller
    {
        private readonly IDisbursementService _disbursementService;
        private readonly IBenefitService _benefitService;

        public DisbursementController(IDisbursementService disbursementService, IBenefitService benefitService)
        {
            _disbursementService = disbursementService;
            _benefitService = benefitService;
        }

        // GET: Disbursement
        public async Task<IActionResult> Index()
        {
            var disbursements = await _disbursementService.GetAllDisbursementsAsync();
            return View(disbursements);
        }

        // GET: Disbursement/History
        public async Task<IActionResult> History(DateTime? startDate, DateTime? endDate, string? benefitType, int? officerId, string? status)
        {
            var disbursements = await _disbursementService.GetAllDisbursementsAsync();
            var benefits = await _benefitService.GetAllBenefitsAsync();

            // Apply filters
            if (startDate.HasValue)
            {
                disbursements = disbursements.Where(d => d.Date >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                disbursements = disbursements.Where(d => d.Date <= endDate.Value);
            }
            if (!string.IsNullOrEmpty(benefitType))
            {
                disbursements = disbursements.Where(d => d.Benefit != null && d.Benefit.Type == benefitType);
            }
            if (officerId.HasValue)
            {
                disbursements = disbursements.Where(d => d.OfficerID == officerId.Value);
            }
            if (!string.IsNullOrEmpty(status))
            {
                disbursements = disbursements.Where(d => d.Status == status);
            }

            // Populate filter dropdowns
            ViewBag.BenefitTypes = benefits.Select(b => b.Type).Distinct().ToList();
            ViewBag.Statuses = new List<string> { "Completed", "Pending", "Failed" };
            ViewBag.OfficerIds = disbursements.Select(d => d.OfficerID).Distinct().OrderBy(o => o).ToList();

            // Pass current filter values
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.SelectedBenefitType = benefitType;
            ViewBag.SelectedOfficerId = officerId;
            ViewBag.SelectedStatus = status;

            return View(disbursements.ToList());
        }

        // GET: Disbursement/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var disbursement = await _disbursementService.GetDisbursementByIdAsync(id.Value);
            if (disbursement == null)
            {
                return NotFound();
            }

            return View(disbursement);
        }

        // GET: Disbursement/Create
        public async Task<IActionResult> Create()
        {
            await PopulateBenefitDropdown();
            return View();
        }

        // POST: Disbursement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DisbursementID,BenefitID,CitizenID,OfficerID,Amount,Date,Status")] Disbursement disbursement)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _disbursementService.CreateDisbursementAsync(disbursement);
                    return RedirectToAction(nameof(Index));
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            await PopulateBenefitDropdown(disbursement.BenefitID);
            return View(disbursement);
        }

        // GET: Disbursement/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var disbursement = await _disbursementService.GetDisbursementByIdAsync(id.Value);
            if (disbursement == null)
            {
                return NotFound();
            }

            await PopulateBenefitDropdown(disbursement.BenefitID);
            return View(disbursement);
        }

        // POST: Disbursement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DisbursementID,BenefitID,CitizenID,OfficerID,Amount,Date,Status")] Disbursement disbursement)
        {
            if (id != disbursement.DisbursementID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (!await _disbursementService.DisbursementExistsAsync(disbursement.DisbursementID))
                {
                    return NotFound();
                }

                try
                {
                    await _disbursementService.UpdateDisbursementAsync(disbursement);
                    return RedirectToAction(nameof(Index));
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            await PopulateBenefitDropdown(disbursement.BenefitID);
            return View(disbursement);
        }

        // GET: Disbursement/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var disbursement = await _disbursementService.GetDisbursementByIdAsync(id.Value);
            if (disbursement == null)
            {
                return NotFound();
            }

            return View(disbursement);
        }

        // POST: Disbursement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _disbursementService.DeleteDisbursementAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateBenefitDropdown(int? selectedBenefitId = null)
        {
            var benefits = await _benefitService.GetAllBenefitsAsync();
            var benefitList = benefits.Select(b => new SelectListItem
            {
                Value = b.BenefitID.ToString(),
                Text = $"{b.BenefitID} - {b.Type} - {b.Amount:C}",
                Selected = b.BenefitID == selectedBenefitId
            });
            ViewData["BenefitID"] = new SelectList(benefitList, "Value", "Text", selectedBenefitId);
        }
    }
}