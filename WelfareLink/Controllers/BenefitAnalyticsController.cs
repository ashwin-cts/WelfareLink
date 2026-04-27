using Microsoft.AspNetCore.Mvc;
using WelfareLink.Services;
using WelfareLink.ViewModels;

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
            try
            {
                var viewModel = await _api.GetBenefitAnalyticsDashboardAsync();
                if (viewModel == null)
                {
                    ViewBag.ErrorMessage = "Unable to load analytics data. Please try again later.";
                    return View(new AnalyticsDashboardViewModel());
                }
                return View(viewModel);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                ViewBag.ErrorMessage = "Analytics service is not available. Please ensure the Analytics API is running.";
                return View(new AnalyticsDashboardViewModel());
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"An error occurred while loading analytics: {ex.Message}";
                return View(new AnalyticsDashboardViewModel());
            }
        }
    }
}

