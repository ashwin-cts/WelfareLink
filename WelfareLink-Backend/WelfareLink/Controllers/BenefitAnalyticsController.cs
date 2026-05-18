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

                // If analytics API returns null, build dashboard from actual data
                if (viewModel == null)
                {
                    var benefits = (await _api.GetAllBenefitsAsync()).ToList();
                    var disbursements = (await _api.GetAllDisbursementsAsync()).ToList();

                    // Group by benefit type for breakdown
                    var benefitBreakdowns = new List<BenefitTypeBreakdown>();
                    foreach (var benefit in benefits.GroupBy(b => b.Type))
                    {
                        var benefitsInType = benefit.ToList();
                        var disbursementsForType = disbursements.Where(d => benefitsInType.Any(b => b.BenefitID == d.BenefitID)).ToList();

                        benefitBreakdowns.Add(new BenefitTypeBreakdown
                        {
                            Type = benefit.Key ?? "Unknown",
                            Count = benefitsInType.Count,
                            TotalAmount = benefitsInType.Sum(b => b.Amount),
                            DisbursedAmount = disbursementsForType.Where(d => d.Status == "Completed").Sum(d => d.Amount),
                            DisbursedCount = disbursementsForType.Count(d => d.Status == "Completed"),
                            Percentage = benefits.Count > 0 ? (double)benefitsInType.Count / benefits.Count * 100 : 0
                        });
                    }

                    viewModel = new AnalyticsDashboardViewModel
                    {
                        TotalAllocated = benefits.Count,
                        TotalDisbursed = disbursements.Count(d => d.Status == "Completed"),
                        TotalPending = disbursements.Count(d => d.Status == "Pending"),
                        TotalFailed = disbursements.Count(d => d.Status == "Failed"),
                        TotalAmountAllocated = benefits.Sum(b => b.Amount),
                        TotalAmountDisbursed = disbursements.Where(d => d.Status == "Completed").Sum(d => d.Amount),
                        DisbursementEfficiency = disbursements.Count > 0 ? (double)disbursements.Count(d => d.Status == "Completed") / disbursements.Count * 100 : 0,
                        AllocationRate = benefits.Count > 0 ? (double)disbursements.Count / benefits.Count * 100 : 0,
                        BenefitTypeBreakdowns = benefitBreakdowns
                    };
                }

                return View(viewModel ?? new AnalyticsDashboardViewModel());
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

