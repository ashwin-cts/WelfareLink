using Microsoft.AspNetCore.Mvc;
using WelfareLink.Interfaces;
using WelfareLink.Models; // Ensure this points to where ComplianceRecord is stored

namespace WelfareLink.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComplainceRecordController : ControllerBase
    {
        private readonly IComplainceRecordService _complainceRecordService;

        public ComplainceRecordController(IComplainceRecordService complainceRecordService)
        {
            _complainceRecordService = complainceRecordService;
        }

        // GET: api/ComplainceRecord
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ComplianceRecord>>> GetAllRecords()
        {
            var records = await _complainceRecordService.GetAllRecordsAsync();
            return Ok(records);
        }

        // GET: api/ComplainceRecord/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ComplianceRecord>> GetRecordById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("Compliance Record ID is required.");
            }

            var record = await _complainceRecordService.GetRecordByIdAsync(id);

            if (record == null)
            {
                return NotFound($"Compliance record with ID '{id}' was not found.");
            }

            return Ok(record);
        }

        // GET: api/ComplainceRecord/entity/{entityId}
        [HttpGet("entity/{entityId}")]
        public async Task<ActionResult<IEnumerable<ComplianceRecord>>> GetRecordsByEntity(string entityId)
        {
            if (string.IsNullOrWhiteSpace(entityId))
            {
                return BadRequest("Entity ID is required.");
            }

            var records = await _complainceRecordService.GetRecordsByEntityAsync(entityId);
            return Ok(records);
        }

        // POST: api/ComplainceRecord
        [HttpPost]
        public async Task<ActionResult<ComplianceRecord>> CreateRecord([FromBody] ComplianceRecord record)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _complainceRecordService.CreateRecordAsync(record);

            if (!success)
            {
                return BadRequest("Could not create the compliance record. Please check server logs.");
            }

            return CreatedAtAction(nameof(GetRecordById), new { id = record.ComplianceID }, record);
        }
    }
}
