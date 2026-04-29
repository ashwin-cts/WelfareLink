using Microsoft.AspNetCore.Mvc;
using WelfareLink.WelfareOfficerManagement.API.Interfaces;
using WelfareLink.WelfareOfficerManagement.API.Models;

namespace WelfareLink.WelfareOfficerManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WelfareApplicationApiController : ControllerBase
    {
        private readonly IWelfareApplicationService _welfareApplicationService;
        private readonly IWelfareProgramService _welfareProgramService;

        public WelfareApplicationApiController(
            IWelfareApplicationService welfareApplicationService,
            IWelfareProgramService welfareProgramService)
        {
            _welfareApplicationService = welfareApplicationService;
            _welfareProgramService = welfareProgramService;
        }

        // GET: api/welfareapplicationapi
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? status = null)
        {
            var applications = await _welfareApplicationService.GetAllApplicationsAsync();

            if (!string.IsNullOrEmpty(status))
                applications = applications.Where(a => a.Status.Equals(status, StringComparison.OrdinalIgnoreCase));

            return Ok(applications);
        }

        // GET: api/welfareapplicationapi/pending
        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            var applications = await _welfareApplicationService.GetPendingApplicationsAsync();
            return Ok(applications);
        }

        // GET: api/welfareapplicationapi/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var application = await _welfareApplicationService.GetApplicationByIdAsync(id);
            if (application == null) return NotFound();

            return Ok(application);
        }

        // GET: api/welfareapplicationapi/date-range
        [HttpGet("date-range")]
        public async Task<IActionResult> GetByDateRange([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
        {
            var applications = await _welfareApplicationService.GetApplicationsByDateRangeAsync(startDate, endDate);
            return Ok(applications);
        }

        // GET: api/welfareapplicationapi/summary
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var summary = await _welfareApplicationService.GetApplicationStatusSummaryAsync();
            var pendingCount = await _welfareApplicationService.GetPendingApplicationCountAsync();
            return Ok(new { Summary = summary, PendingCount = pendingCount });
        }

        // POST: api/welfareapplicationapi
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] WelfareApplication application)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var created = await _welfareApplicationService.CreateApplicationAsync(application);
            return Ok(new { Message = $"Application #{created.ApplicationID} created successfully.", Application = created });
        }

        // PUT: api/welfareapplicationapi/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] WelfareApplication application)
        {
            if (id != application.ApplicationID) return BadRequest(new { Error = "ID mismatch." });
            if (!await _welfareApplicationService.ApplicationExistsAsync(id)) return NotFound();

            await _welfareApplicationService.UpdateApplicationAsync(application);
            return Ok(new { Message = $"Application #{id} updated successfully." });
        }

        // PATCH: api/welfareapplicationapi/{id}/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            var result = await _welfareApplicationService.UpdateApplicationStatusAsync(id, status);
            if (!result) return BadRequest(new { Error = "Failed to update application status." });
            return Ok(new { Message = $"Application #{id} status updated to {status}." });
        }

        // DELETE: api/welfareapplicationapi/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _welfareApplicationService.ApplicationExistsAsync(id)) return NotFound();

            await _welfareApplicationService.DeleteApplicationAsync(id);
            return Ok(new { Message = $"Application #{id} deleted successfully." });
        }
    }
}
