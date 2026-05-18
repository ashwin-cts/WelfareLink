using Microsoft.AspNetCore.Authorization; // ADDED FOR AUTHORIZATION
using Microsoft.AspNetCore.Mvc;
using WelfareLink.WelfareOfficerManagement.API.Interfaces;
using WelfareLink.WelfareOfficerManagement.API.Models;

namespace WelfareLink.WelfareOfficerManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // Base Rule: Internal staff can view eligibility checks for processing, metrics, and audits
    [Authorize(Roles = "Admin,WelfareOfficer,ProgramManager,GovernmentAuditor,ComplianceOfficer")]
    public class EligibilityCheckApiController : ControllerBase
    {
        private readonly IEligibilityCheckService _eligibilityCheckService;
        private readonly IWelfareApplicationService _applicationService;
        private readonly ICitizenService _citizenService;
        private readonly ICitizenDocumentService _documentService;

        public EligibilityCheckApiController(
            IEligibilityCheckService eligibilityCheckService,
            IWelfareApplicationService applicationService,
            ICitizenService citizenService,
            ICitizenDocumentService documentService)
        {
            _eligibilityCheckService = eligibilityCheckService;
            _applicationService = applicationService;
            _citizenService = citizenService;
            _documentService = documentService;
        }

        // GET: api/eligibilitycheckapi
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var checks = await _eligibilityCheckService.GetAllChecksAsync();
            return Ok(checks);
        }

        // GET: api/eligibilitycheckapi/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var check = await _eligibilityCheckService.GetCheckByIdAsync(id);
            if (check == null) return NotFound();
            return Ok(check);
        }

        // GET: api/eligibilitycheckapi/application/{applicationId}
        [HttpGet("application/{applicationId}")]
        public async Task<IActionResult> GetByApplicationId(int applicationId)
        {
            var checks = await _eligibilityCheckService.GetChecksByApplicationIdAsync(applicationId);
            return Ok(checks);
        }

        // GET: api/eligibilitycheckapi/application/{applicationId}/latest
        [HttpGet("application/{applicationId}/latest")]
        public async Task<IActionResult> GetLatestForApplication(int applicationId)
        {
            var check = await _eligibilityCheckService.GetLatestCheckForApplicationAsync(applicationId);
            if (check == null) return NotFound();
            return Ok(check);
        }

        // GET: api/eligibilitycheckapi/summary
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var summary = await _eligibilityCheckService.GetEligibilityResultSummaryAsync();
            var rate = await _eligibilityCheckService.GetEligibilityRateAsync();
            return Ok(new { Summary = summary, EligibilityRate = rate });
        }

        // GET: api/eligibilitycheckapi/application-info/{applicationId}
        [HttpGet("application-info/{applicationId}")]
        public async Task<IActionResult> GetApplicationInfo(int applicationId)
        {
            var application = await _applicationService.GetApplicationByIdAsync(applicationId);
            if (application == null) return NotFound();

            var citizen = await _citizenService.GetCitizenByIdAsync(application.CitizenID);
            var documents = await _documentService.GetDocumentsByCitizenIdAsync(application.CitizenID);

            return Ok(new
            {
                Application = application,
                Citizen = citizen,
                Documents = documents
            });
        }

        // POST: api/eligibilitycheckapi
        [HttpPost]
        // OVERRIDE: Only Welfare Officers and Admins can create eligibility checks
        [Authorize(Roles = "WelfareOfficer")]
        public async Task<IActionResult> Create([FromBody] EligibilityCheck check, [FromQuery] int? applicationId = null)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var created = await _eligibilityCheckService.CreateCheckAsync(check, applicationId);
            return Ok(new { Message = "Eligibility check recorded successfully.", Check = created });
        }

        // PUT: api/eligibilitycheckapi/{id}
        [HttpPut("{id}")]
        // OVERRIDE: Only Welfare Officers and Admins can update eligibility checks
        [Authorize(Roles = "Admin,WelfareOfficer")]
        public async Task<IActionResult> Update(int id, [FromBody] EligibilityCheck check)
        {
            if (id != check.CheckID) return BadRequest(new { Error = "ID mismatch." });
            if (!await _eligibilityCheckService.CheckExistsAsync(id)) return NotFound();

            await _eligibilityCheckService.UpdateCheckAsync(check);
            return Ok(new { Message = $"Eligibility check #{id} updated successfully." });
        }

        // DELETE: api/eligibilitycheckapi/{id}
        [HttpDelete("{id}")]
        // OVERRIDE: Only Welfare Officers and Admins can delete eligibility checks
        [Authorize(Roles = "Admin,WelfareOfficer")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _eligibilityCheckService.CheckExistsAsync(id)) return NotFound();

            await _eligibilityCheckService.DeleteCheckAsync(id);
            return Ok(new { Message = $"Eligibility check #{id} deleted successfully." });
        }
    }
}