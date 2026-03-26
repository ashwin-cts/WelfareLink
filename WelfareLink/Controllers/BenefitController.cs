using Microsoft.AspNetCore.Mvc;
using WelfareLink.Interfaces;
using WelfareLink.Models;

namespace WelfareLink.Controllers
{
    public class BenefitController : Controller
    {
        private readonly IBenefitService _benefitService;

        public BenefitController(IBenefitService benefitService)
        {
            _benefitService = benefitService;
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
        public IActionResult Create()
        {
            return View();
        }

        // POST: Benefit/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BenefitID,ApplicationID,Type,Amount,Date,Status")] Benefit benefit)
        {
            if (ModelState.IsValid)
            {
                await _benefitService.CreateBenefitAsync(benefit);
                return RedirectToAction(nameof(Index));
            }
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

                await _benefitService.UpdateBenefitAsync(benefit);
                return RedirectToAction(nameof(Index));
            }
            return View(benefit);
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
