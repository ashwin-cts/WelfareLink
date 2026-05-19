using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WelfareLink.ComplianceAndAuditLog.API.Interfaces;
using WelfareLink.ComplianceAndAuditLog.API.Models;

namespace WelfareLink.ComplianceAndAuditLog.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // FIX: Just require the user to be logged in at the controller level.
    // We will handle the specific role restrictions on the individual endpoints.
    [Authorize]
    public class ComplainceRecordApiController : ControllerBase
    {
        private readonly IComplainceRecordService _complainceRecordService;

        public ComplainceRecordApiController(IComplainceRecordService complainceRecordService)
        {
            _complainceRecordService = complainceRecordService;
        }

        // GET: api/complaincerecordapi
        [HttpGet]
        // Allow Welfare Officers to see compliance records for the dashboard UI
        [Authorize(Roles = "Admin,ComplianceOfficer,GovernmentAuditor,WelfareOfficer")]
        public async Task<IActionResult> GetAll()
            => Ok(await _complainceRecordService.GetAllRecordsAsync());

        // GET: api/complaincerecordapi/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,ComplianceOfficer,GovernmentAuditor")]
        public async Task<IActionResult> GetById(int id)
        {
            var record = await _complainceRecordService.GetRecordByIdAsync(id);
            return record == null ? NotFound() : Ok(record);
        }

        // GET: api/complaincerecordapi/open
        [HttpGet("open")]
        [Authorize(Roles = "Admin,ComplianceOfficer,GovernmentAuditor")]
        public async Task<IActionResult> GetOpen()
            => Ok(await _complainceRecordService.GetOpenRecordsAsync());

        // POST: api/complaincerecordapi
        [HttpPost]
        // Explicitly restrict Creation (POST) to only Admins and Compliance Officers
        [Authorize(Roles = "Admin,ComplianceOfficer")]
        public async Task<IActionResult> Create([FromBody] ComplainceRecord record)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var created = await _complainceRecordService.CreateRecordAsync(record);
                return CreatedAtAction(nameof(GetById), new { id = created.RecordID }, created);
            }
            catch (Exception ex) { return BadRequest(new { Error = ex.Message }); }
        }

        // PATCH: api/complaincerecordapi/{id}/status
        [HttpPatch("{id}/status")]
        // Explicitly restrict Edits/Updates to only Admins and Compliance Officers
        [Authorize(Roles = "Admin,ComplianceOfficer")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateComplianceStatusRequest req)
        {
            try
            {
                var updated = await _complainceRecordService.UpdateStatusAsync(id, req.Status, req.ResolvedByUserId, req.Notes);
                return Ok(updated);
            }
            catch (InvalidOperationException ex) { return NotFound(new { Error = ex.Message }); }
        }
    }

    public record UpdateComplianceStatusRequest(string Status, int? ResolvedByUserId, string? Notes);
}