using Microsoft.AspNetCore.Mvc;
using WelfareLinkApi.Interfaces;
using WelfareLinkApi.Models;

namespace WelfareLinkApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
                ProgramBudget = a.Program?.Budget,
                ProgramStatus = a.Program?.Status ?? "-",
                SubmittedDate = a.SubmittedDate.ToString("dd MMM yyyy"),
                a.Status
            });

            return Ok(new { Dropdown = dropdownItems, Applications = detailedItems });
        }

        // POST: api/benefit
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Benefit benefit, [FromQuery] int officerId = 0)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var created = await _benefitService.CreateBenefitAsync(benefit, officerId);

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
        public async Task<IActionResult> Update(int id, [FromBody] Benefit benefit, [FromQuery] int officerId = 0)
        {
            if (id != benefit.BenefitID) return BadRequest(new { Error = "ID mismatch" });
            if (!await _benefitService.BenefitExistsAsync(id)) return NotFound();

            try
            {
                var updated = await _benefitService.UpdateBenefitAsync(benefit, officerId);

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
