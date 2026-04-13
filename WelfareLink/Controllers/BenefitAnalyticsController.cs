using Microsoft.AspNetCore.Mvc;
using WelfareLink.Services;

namespace WelfareLink.Controllers
{
    public class BenefitAnalyticsController : Controller
    {
        private readonly WelfareApiClient _api;

        public BenefitAnalyticsController(WelfareApiClient api)
        {
            _api = api;
        }

        // GET: Analytics/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var viewModel = await _api.GetBenefitAnalyticsDashboardAsync();
            return View(viewModel);
        }
    }
}

