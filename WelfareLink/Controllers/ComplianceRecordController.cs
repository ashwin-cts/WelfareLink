using Microsoft.AspNetCore.Mvc;
using WelfareLink.Interfaces;
using WelfareLink.Models;

namespace WelfareLink.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComplianceRecordController : ControllerBase
    {
        private readonly IComplianceRecordService _complianceRecordService;

        public ComplianceRecordController(IComplianceRecordService complianceRecordService)
        {
            _complianceRecordService = complianceRecordService;
        }

        // GET: api/ComplianceRecord
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ComplianceRecord>>> GetAllRecords()
        {
            var records = await _complianceRecordService.GetAllRecordsAsync();
            return Ok(records);
        }

        // GET: api/ComplianceRecord/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ComplianceRecord>> GetRecordById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("Compliance Record ID is required.");
            }

            var record = await _complianceRecordService.GetRecordByIdAsync(id);

            if (record == null)
            {
                return NotFound($"Compliance record with ID '{id}' was not found.");
            }

            return Ok(record);
        }

        // GET: api/ComplianceRecord/entity/{entityId}
        [HttpGet("entity/{entityId}")]
        public async Task<ActionResult<IEnumerable<ComplianceRecord>>> GetRecordsByEntity(string entityId)
        {
            if (string.IsNullOrWhiteSpace(entityId))
            {
                return BadRequest("Entity ID is required.");
            }

            var records = await _complianceRecordService.GetRecordsByEntityAsync(entityId);
            return Ok(records);
        }

        // POST: api/ComplianceRecord
        [HttpPost]
        public async Task<ActionResult<ComplianceRecord>> CreateRecord([FromBody] ComplianceRecord record)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _complianceRecordService.CreateRecordAsync(record);

            if (!success)
            {
                return BadRequest("Could not create the compliance record. Please check server logs.");
            }

            return CreatedAtAction(nameof(GetRecordById), new { id = record.ComplianceID }, record);
        }
    }
}
