using Microsoft.AspNetCore.Authorization; // ADDED FOR AUTHORIZATION
using Microsoft.AspNetCore.Mvc;
using WelfareLink.WelfareOfficerManagement.API.Interfaces;

namespace WelfareLink.WelfareOfficerManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // Base Rule: Internal staff tracking money flow and performance can view analytics
    [Authorize(Roles = "Admin,WelfareOfficer,ProgramManager,GovernmentAuditor,ComplianceOfficer")]
    public class BenefitAnalyticsApiController : ControllerBase
    {
        private readonly IBenefitAnalyticsService _analyticsService;

        public BenefitAnalyticsApiController(IBenefitAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        // GET: api/benefitanalyticsapi/dashboard
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var viewModel = await _analyticsService.GetDashboardDataAsync();
            return Ok(viewModel);
        }

        // GET: api/benefitanalyticsapi/summary
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var totalAllocated = await _analyticsService.GetTotalAllocatedCountAsync();
            var totalDisbursed = await _analyticsService.GetTotalDisbursedCountAsync();
            var totalPending = await _analyticsService.GetTotalPendingCountAsync();
            var totalAmount = await _analyticsService.GetTotalAmountAllocatedAsync();
            var efficiency = await _analyticsService.GetDisbursementEfficiencyAsync();

            return Ok(new
            {
                TotalAllocated = totalAllocated,
                TotalDisbursed = totalDisbursed,
                TotalPending = totalPending,
                TotalAmountAllocated = totalAmount,
                DisbursementEfficiency = efficiency
            });
        }

        // GET: api/benefitanalyticsapi/type-breakdown
        [HttpGet("type-breakdown")]
        public async Task<IActionResult> GetTypeBreakdown()
        {
            var breakdown = await _analyticsService.GetBenefitTypeBreakdownsAsync();
            return Ok(breakdown);
        }

        // GET: api/benefitanalyticsapi/recent-disbursements
        [HttpGet("recent-disbursements")]
        public async Task<IActionResult> GetRecentDisbursements([FromQuery] int count = 5)
        {
            var recent = await _analyticsService.GetRecentDisbursementsAsync(count);
            return Ok(recent);
        }

        // GET: api/benefitanalyticsapi/monthly-trends
        [HttpGet("monthly-trends")]
        public async Task<IActionResult> GetMonthlyTrends([FromQuery] int months = 6)
        {
            var trends = await _analyticsService.GetMonthlyTrendsAsync(months);
            return Ok(trends);
        }
    }
}