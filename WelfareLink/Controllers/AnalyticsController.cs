using Microsoft.AspNetCore.Mvc;
using WelfareLink.Interfaces;

namespace WelfareLink.Controllers
{
    public class AnalyticsController : Controller
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
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
