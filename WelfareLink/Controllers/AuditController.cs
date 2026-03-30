using Microsoft.AspNetCore.Mvc;
using WelfareLink.Interfaces;
using WelfareLink.Models; // Ensure this matches your Audit model namespace

namespace WelfareLink.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditController : ControllerBase
    {
        private readonly IAuditService _auditService;

        public AuditController(IAuditService auditService)
        {
            _auditService = auditService;
        }

        // GET: api/Audit
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Audit>>> GetAudits()
        {
            var audits = await _auditService.GetAllAuditsAsync();
            return Ok(audits);
        }

        // GET: api/Audit/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Audit>> GetAudit(string id)
        {
            var audit = await _auditService.GetAuditByIdAsync(id);

            if (audit == null)
            {
                return NotFound($"Audit with ID {id} not found.");
            }

            return Ok(audit);
        }

        // POST: api/Audit
        [HttpPost]
        public async Task<ActionResult<Audit>> CreateAudit([FromBody] Audit audit)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _auditService.CreateAuditAsync(audit);

            if (!success)
            {
                return BadRequest("Unable to create the audit record.");
            }

            return CreatedAtAction(nameof(GetAudit), new { id = audit.AuditID }, audit);
        }

        // PUT: api/Audit/{id}/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateAuditStatus(string id, [FromBody] AuditUpdateDto updateDto)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Audit ID is required.");
            }

            var success = await _auditService.UpdateAuditStatusAsync(id, updateDto.Status, updateDto.Findings);

            if (!success)
            {
                return NotFound($"Unable to find or update Audit with ID {id}.");
            }

            return NoContent();
        }
    }

    // A lightweight Data Transfer Object (DTO) for cleaner PUT payloads
    public class AuditUpdateDto
    {
        public string Status { get; set; }
        public string Findings { get; set; }
    }
}