using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WelfareLink.Interfaces;
using WelfareLink.Models;
using WelfareLink.Data;

namespace WelfareLink.API_Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class CitizenApiController : ControllerBase
    {
        private readonly ICitizenService _citizenService;
        private readonly ICitizenRepository _citizenRepository;
        private readonly ICitizenDocumentService _documentService;
        private readonly IWelfareApplicationService _applicationService;
        private readonly IWelfareProgramService _programService;
        private readonly WelfareLinkDbContext _context;

        public CitizenApiController(
            ICitizenService citizenService,
            ICitizenRepository citizenRepository,
            ICitizenDocumentService documentService,
            IWelfareApplicationService applicationService,
            IWelfareProgramService programService,
            WelfareLinkDbContext context)
        {
            _citizenService = citizenService;
            _citizenRepository = citizenRepository;
            _documentService = documentService;
            _applicationService = applicationService;
            _programService = programService;
            _context = context;
        }

        // GET: api/citizen
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Citizen>>> GetAllCitizens()
        {
            try
            {
                var citizens = await _context.Citizens.ToListAsync();
                return Ok(citizens);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving citizens", error = ex.Message });
            }
        }

        // GET: api/citizen/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Citizen>> GetCitizenById(int id)
        {
            try
            {
                var citizen = await _citizenService.GetCitizenByIdAsync(id);
                if (citizen == null)
                    return NotFound(new { message = $"Citizen with ID {id} not found" });

                return Ok(citizen);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving citizen", error = ex.Message });
            }
        }

        // GET: api/citizen/user/5
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<Citizen>> GetCitizenByUserId(int userId)
        {
            try
            {
                var citizen = await _citizenService.GetCitizenByUserIdAsync(userId);
                if (citizen == null)
                    return NotFound(new { message = $"Citizen with User ID {userId} not found" });

                return Ok(citizen);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving citizen", error = ex.Message });
            }
        }

        // POST: api/citizen
        [HttpPost]
        public async Task<ActionResult<Citizen>> CreateCitizen([FromBody] Citizen citizen)
        {
            if (citizen == null)
                return BadRequest(new { message = "Citizen data is required" });

            try
            {
                var success = await _citizenService.CreateCitizenProfileAsync(citizen);
                if (success)
                {
                    var createdCitizen = await _citizenService.GetCitizenByIdAsync(citizen.CitizenId);
                    return CreatedAtAction(nameof(GetCitizenById), new { id = citizen.CitizenId }, createdCitizen);
                }

                return BadRequest(new { message = "Failed to create citizen profile" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating citizen", error = ex.Message });
            }
        }

        // PUT: api/citizen/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCitizen(int id, [FromBody] Citizen citizen)
        {
            if (citizen == null)
                return BadRequest(new { message = "Citizen data is required" });

            if (id != citizen.CitizenId)
                return BadRequest(new { message = "ID mismatch" });

            try
            {
                var existingCitizen = await _citizenService.GetCitizenByIdAsync(id);
                if (existingCitizen == null)
                    return NotFound(new { message = $"Citizen with ID {id} not found" });

                var success = await _citizenService.UpdateCitizenProfileAsync(citizen);
                if (success)
                    return Ok(new { message = "Citizen profile updated successfully" });

                return BadRequest(new { message = "Failed to update citizen profile" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating citizen", error = ex.Message });
            }
        }

        // GET: api/citizen/5/documents
        [HttpGet("{citizenId}/documents")]
        public async Task<ActionResult<IEnumerable<CitizenDocument>>> GetCitizenDocuments(int citizenId)
        {
            try
            {
                var documents = await _documentService.GetDocumentsByCitizenIdAsync(citizenId);
                return Ok(documents);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving documents", error = ex.Message });
            }
        }

        // GET: api/citizen/5/applications
        [HttpGet("{citizenId}/applications")]
        public async Task<ActionResult<IEnumerable<WelfareApplication>>> GetCitizenApplications(int citizenId)
        {
            try
            {
                var allApplications = await _applicationService.GetAllApplicationsAsync();
                var citizenApplications = allApplications.Where(a => a.CitizenID == citizenId);
                return Ok(citizenApplications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving applications", error = ex.Message });
            }
        }

        // DELETE: api/citizen/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCitizen(int id)
        {
            try
            {
                var citizen = await _citizenService.GetCitizenByIdAsync(id);
                if (citizen == null)
                    return NotFound(new { message = $"Citizen with ID {id} not found" });

                // Update status to inactive instead of deleting
                citizen.Status = "Inactive";
                var success = await _citizenService.UpdateCitizenProfileAsync(citizen);

                if (success)
                    return Ok(new { message = "Citizen profile deleted successfully" });

                return BadRequest(new { message = "Failed to delete citizen profile" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting citizen", error = ex.Message });
            }
        }
    }
}
