using Microsoft.AspNetCore.Authorization; // ADDED FOR AUTHORIZATION
using System.Security.Claims; // ADDED FOR JWT CLAIM EXTRACTION
using Microsoft.AspNetCore.Mvc;
using WelfareLink.WelfareOfficerManagement.API.Interfaces;
using WelfareLink.WelfareOfficerManagement.API.Models;

namespace WelfareLink.WelfareOfficerManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // Base Rule: Internal staff can view benefits for processing, metrics, and audits
    [Authorize(Roles = "Admin,WelfareOfficer,ProgramManager,GovernmentAuditor,ComplianceOfficer")]
    public class BenefitApiController : ControllerBase
    {
        private readonly IBenefitService _benefitService;
        private readonly IWelfareApplicationService _welfareApplicationService;
        private readonly IResourceService _resourceService;

        public BenefitApiController(
            IBenefitService benefitService,
            IWelfareApplicationService welfareApplicationService,
            IResourceService resourceService)
        {
            _benefitService = benefitService;
            _welfareApplicationService = welfareApplicationService;
            _resourceService = resourceService;
        }

        // Helper method to securely extract UserId from JWT token
        private int GetCurrentUserId()
        {
            var userIdClaim = HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? HttpContext?.User.FindFirst("UserId")?.Value;

            return int.TryParse(userIdClaim, out int id) ? id : 0;
        }

        // GET: api/benefit
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var benefits = await _benefitService.GetAllBenefitsAsync();
            return Ok(benefits);
        }

        // GET: api/benefit/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var benefit = await _benefitService.GetBenefitByIdAsync(id);
            if (benefit == null) return NotFound();
            return Ok(benefit);
        }

        // GET: api/benefit/filter
        /// <summary>
        /// Get benefits filtered by status and/or date range
        /// </summary>
        [HttpGet("filter")]
        public async Task<IActionResult> GetFilteredBenefits(
            [FromQuery] string? status = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int? applicationId = null)
        {
            var benefits = await _benefitService.GetAllBenefitsAsync();

            if (!string.IsNullOrEmpty(status) && !status.Equals("All", StringComparison.OrdinalIgnoreCase))
                benefits = benefits.Where(b => b.Status?.Equals(status, StringComparison.OrdinalIgnoreCase) ?? false).ToList();

            if (fromDate.HasValue)
                benefits = benefits.Where(b => b.Date >= fromDate.Value).ToList();

            if (toDate.HasValue)
                benefits = benefits.Where(b => b.Date <= toDate.Value).ToList();

            if (applicationId.HasValue)
                benefits = benefits.Where(b => b.ApplicationID == applicationId.Value).ToList();

            var result = benefits
                .OrderByDescending(b => b.Date)
                .Select(b => new
                {
                    b.BenefitID,
                    b.Amount,
                    b.Type,
                    b.Status,
                    b.Date,
                    Application = new
                    {
                        b.WelfareApplication?.ApplicationID,
                        b.WelfareApplication?.Status
                    },
                    Citizen = new
                    {
                        b.WelfareApplication?.Citizen?.CitizenId,
                        b.WelfareApplication?.Citizen?.Name,
                        b.WelfareApplication?.Citizen?.ContactInfo
                    },
                    Program = new
                    {
                        b.WelfareApplication?.Program?.ProgramID,
                        b.WelfareApplication?.Program?.Title,
                        b.WelfareApplication?.Program?.Budget,
                        b.WelfareApplication?.Program?.MaxBenefitPerCitizen
                    }
                });

            return Ok(result);
        }

        // GET: api/benefit/pending
        /// <summary>
        /// Get all pending benefits
        /// </summary>
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingBenefits()
        {
            return await GetFilteredBenefits(status: "Pending");
        }

        // GET: api/benefit/dropdown
        [HttpGet("dropdown")]
        public async Task<IActionResult> PopulateApplicationDropdown(int? selectedId = null)
        {
            var applications = await _welfareApplicationService.GetAllApplicationsAsync();

            var appList = applications
                .Where(a => a.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var dropdownItems = appList.Select(a => new
            {
                a.ApplicationID,
                Display = $"App #{a.ApplicationID} | {a.Citizen?.Name ?? $"Citizen #{a.CitizenID}"} | {a.Program?.Title ?? $"Program #{a.ProgramID}"}",
                Selected = (selectedId.HasValue && a.ApplicationID == selectedId.Value)
            });

            var detailedItems = appList.Select(a => new
            {
                a.ApplicationID,
                a.CitizenID,
                CitizenName = a.Citizen?.Name ?? "-",
                a.ProgramID,
                ProgramTitle = a.Program?.Title ?? $"Program #{a.ProgramID}",
                ProgramDesc = a.Program?.Description ?? "-",
                ProgramMaxBenefit = a.Program?.MaxBenefitPerCitizen,
                ProgramBudget = a.Program?.Budget,
                ProgramStatus = a.Program?.Status ?? "-",
                SubmittedDate = a.SubmittedDate.ToString("dd MMM yyyy"),
                a.Status
            });

            return Ok(new { Dropdown = dropdownItems, Applications = detailedItems });
        }

        // POST: api/benefit
        [HttpPost]
        // OVERRIDE: Only Welfare Officers and Admins can allocate benefits
        [Authorize(Roles ="WelfareOfficer")]
        public async Task<IActionResult> Create([FromBody] Benefit benefit, [FromQuery] int officerId = 0)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                // SECURITY FIX: Ignore the query parameter and use the true JWT token identity
                int trueOfficerId = GetCurrentUserId();

                var created = await _benefitService.CreateBenefitAsync(benefit, trueOfficerId);

                return Ok(new
                {
                    Message = $"Benefit #{created.BenefitID} created successfully.",
                    Benefit = created
                });
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        // PUT: api/benefit/{id}
        [HttpPut("{id}")]
        // OVERRIDE: Only Welfare Officers and Admins can modify benefits
        [Authorize(Roles = "Admin,WelfareOfficer")]
        public async Task<IActionResult> Update(int id, [FromBody] Benefit benefit, [FromQuery] int officerId = 0)
        {
            if (id != benefit.BenefitID) return BadRequest(new { Error = "ID mismatch" });
            if (!await _benefitService.BenefitExistsAsync(id)) return NotFound();

            try
            {
                // SECURITY FIX: Ignore the query parameter and use the true JWT token identity
                int trueOfficerId = GetCurrentUserId();

                var updated = await _benefitService.UpdateBenefitAsync(benefit, trueOfficerId);

                return Ok(new
                {
                    Message = $"Benefit #{updated.BenefitID} updated successfully.",
                    Benefit = updated
                });
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        // DELETE: api/benefit/{id}
        [HttpDelete("{id}")]
        // OVERRIDE: Only Welfare Officers and Admins can delete benefits
        [Authorize(Roles = "WelfareOfficer")]
        public async Task<IActionResult> Delete(int id)
        {
            var benefit = await _benefitService.GetBenefitByIdAsync(id);
            if (benefit == null) return NotFound();

            try
            {
                await _benefitService.DeleteBenefitAsync(id);
                return Ok(new { Message = $"Benefit #{id} deleted successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        // GET: api/benefit/program-resource-info/{programId}
        [HttpGet("program-resource-info/{programId}")]
        public async Task<IActionResult> GetProgramResourceInfo(int programId)
        {
            if (programId <= 0) return BadRequest(new { Error = "Invalid program ID." });

            var resources = await _resourceService.GetResourcesByProgramIdAsync(programId);
            var totalResource = (double)resources.Sum(r => r.Quantity);

            var allBenefits = await _benefitService.GetAllBenefitsAsync();
            var alreadyAllocated = allBenefits
                .Where(b => b.WelfareApplication?.ProgramID == programId
                            && !b.Status.Equals("Allocation Pending", StringComparison.OrdinalIgnoreCase)
                            && !b.Status.Equals("Failed", StringComparison.OrdinalIgnoreCase))
                .Sum(b => b.Amount);

            var remainingResource = totalResource - alreadyAllocated;

            return Ok(new
            {
                totalResource,
                alreadyAllocated,
                remainingResource,
                hasResource = totalResource > 0
            });
        }
    }
}