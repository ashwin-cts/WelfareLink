using Microsoft.AspNetCore.Mvc;
using WelfareLinkApi.Interfaces;
using WelfareLinkApi.Models;

namespace WelfareLinkApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditApiController : ControllerBase
    {
        private readonly IAuditService _auditService;

        public AuditApiController(IAuditService auditService)
        {
            _auditService = auditService;
        }

        // GET: api/auditapi
        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _auditService.GetAllAuditsAsync());

        // GET: api/auditapi/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var audit = await _auditService.GetAuditByIdAsync(id);
            return audit == null ? NotFound() : Ok(audit);
        }

        // GET: api/auditapi/dashboard
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
            => Ok(await _auditService.GetGovernmentAuditorDashboardAsync());

        // GET: api/auditapi/program/{programId}
        [HttpGet("program/{programId}")]
        public async Task<IActionResult> GetByProgram(int programId)
            => Ok(await _auditService.GetAuditsByProgramAsync(programId));

        // POST: api/auditapi
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Audit audit)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var created = await _auditService.CreateAuditAsync(audit);
                return CreatedAtAction(nameof(GetById), new { id = created.AuditID }, created);
            }
            catch (Exception ex) { return BadRequest(new { Error = ex.Message }); }
        }

        // PATCH: api/auditapi/{id}/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            try
            {
                var updated = await _auditService.UpdateAuditStatusAsync(id, status);
                return Ok(updated);
            }
            catch (InvalidOperationException ex) { return NotFound(new { Error = ex.Message }); }
        }
    }
}
