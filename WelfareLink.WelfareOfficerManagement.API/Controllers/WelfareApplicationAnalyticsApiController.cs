using Microsoft.AspNetCore.Mvc;
using WelfareLink.WelfareOfficerManagement.API.Interfaces;

namespace WelfareLink.WelfareOfficerManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WelfareApplicationAnalyticsApiController : ControllerBase
    {
        private readonly IWelfareApplicationAnalyticsService _analyticsService;

        public WelfareApplicationAnalyticsApiController(IWelfareApplicationAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        // GET: api/welfareapplicationanalyticsapi/dashboard
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var metrics = await _analyticsService.GetDashboardMetricsAsync();
            return Ok(metrics);
        }

        // GET: api/welfareapplicationanalyticsapi/status-breakdown
        [HttpGet("status-breakdown")]
        public async Task<IActionResult> GetStatusBreakdown()
        {
            var statusBreakdown = await _analyticsService.GetStatusBreakdownAsync();
            var total = statusBreakdown.Values.Sum();

            var statusData = statusBreakdown
                .Select(kvp => new
                {
                    Status = kvp.Key,
                    Count = kvp.Value,
                    Percentage = total > 0 ? (double)kvp.Value / total * 100 : 0
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            return Ok(statusData);
        }

        // GET: api/welfareapplicationanalyticsapi/monthly-trends
        [HttpGet("monthly-trends")]
        public async Task<IActionResult> GetMonthlyTrends([FromQuery] int? year = null)
        {
            var targetYear = year ?? DateTime.Now.Year;
            var trends = await _analyticsService.GetMonthlyTrendsAsync(targetYear);
            return Ok(trends);
        }

        // GET: api/welfareapplicationanalyticsapi/eligibility-report
        [HttpGet("eligibility-report")]
        public async Task<IActionResult> GetEligibilityReport()
        {
            var report = await _analyticsService.GetEligibilityReportAsync();
            return Ok(report);
        }

        // GET: api/welfareapplicationanalyticsapi/approval-rate
        [HttpGet("approval-rate")]
        public async Task<IActionResult> GetApprovalRate()
        {
            var rate = await _analyticsService.GetApprovalRateAsync();
            return Ok(new { ApprovalRate = rate });
        }
    }
}
