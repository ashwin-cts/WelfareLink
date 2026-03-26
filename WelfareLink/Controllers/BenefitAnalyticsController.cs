using Microsoft.AspNetCore.Mvc;
using WelfareLink.Interfaces;

namespace WelfareLink.Controllers
{
    public class BenefitAnalyticsController : Controller
    {
        private readonly IBenefitAnalyticsService _analyticsService;

        public BenefitAnalyticsController(IBenefitAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        // GET: Analytics/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var viewModel = await _analyticsService.GetDashboardDataAsync();
            return View(viewModel);
        }
    }
}
