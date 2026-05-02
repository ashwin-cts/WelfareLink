using Microsoft.AspNetCore.Authorization; // ADDED FOR AUTHORIZATION
using Microsoft.AspNetCore.Mvc;
using WelfareLink.ComplianceAndAuditLog.API.Interfaces;
using WelfareLink.ComplianceAndAuditLog.API.Models;

namespace WelfareLink.ComplianceAndAuditLog.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // Base Rule: Only Admins and Compliance Officers can manage compliance records
    [Authorize(Roles = "Admin,ComplianceOfficer")]
    public class ComplainceRecordApiController : ControllerBase
    {
        private readonly IComplainceRecordService _complainceRecordService;

        public ComplainceRecordApiController(IComplainceRecordService complainceRecordService)
        {
            _complainceRecordService = complainceRecordService;
        }

        // GET: api/complaincerecordapi
        [HttpGet]
        // OVERRIDE: Allow Government Auditors to view compliance records during their audits
        [Authorize(Roles = "Admin,ComplianceOfficer,GovernmentAuditor")]
        public async Task<IActionResult> GetAll()
            => Ok(await _complainceRecordService.GetAllRecordsAsync());

        // GET: api/complaincerecordapi/{id}
        [HttpGet("{id}")]
        // OVERRIDE: Allow Government Auditors to view specific compliance records
        [Authorize(Roles = "Admin,ComplianceOfficer,GovernmentAuditor")]
        public async Task<IActionResult> GetById(int id)
        {
            var record = await _complainceRecordService.GetRecordByIdAsync(id);
            return record == null ? NotFound() : Ok(record);
        }

        // GET: api/complaincerecordapi/open
        [HttpGet("open")]
        // OVERRIDE: Allow Government Auditors to view open compliance records
        [Authorize(Roles = "Admin,ComplianceOfficer,GovernmentAuditor")]
        public async Task<IActionResult> GetOpen()
            => Ok(await _complainceRecordService.GetOpenRecordsAsync());

        // POST: api/complaincerecordapi
        [HttpPost]
        // Falls back to Base Rule: Only Admin & ComplianceOfficer can create records
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
        // Falls back to Base Rule: Only Admin & ComplianceOfficer can resolve/update records
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