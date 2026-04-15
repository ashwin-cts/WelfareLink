using Microsoft.AspNetCore.Mvc;
using WelfareLinkApi.Interfaces;
using WelfareLinkApi.Models;

namespace WelfareLinkApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DisbursementApiController : ControllerBase
    {
        private readonly IDisbursementService _disbursementService;
        private readonly IBenefitService _benefitService;
        private readonly IResourceService _resourceService;

        public DisbursementApiController(
            IDisbursementService disbursementService,
            IBenefitService benefitService,
            IResourceService resourceService)
        {
            _disbursementService = disbursementService;
            _benefitService = benefitService;
            _resourceService = resourceService;
        }

        // GET: api/disbursementapi
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var disbursements = await _disbursementService.GetAllDisbursementsAsync();
            return Ok(disbursements);
        }

        // GET: api/disbursementapi/filter
        /// <summary>
        /// Get disbursements filtered by status and/or date range
        /// </summary>
        [HttpGet("filter")]
        public async Task<IActionResult> GetFilteredDisbursements(
            [FromQuery] string? status = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int? benefitId = null,
            [FromQuery] int? officerId = null)
        {
            var disbursements = await _disbursementService.GetAllDisbursementsAsync();

            if (!string.IsNullOrEmpty(status) && !status.Equals("All", StringComparison.OrdinalIgnoreCase))
                disbursements = disbursements.Where(d => d.Status?.Equals(status, StringComparison.OrdinalIgnoreCase) ?? false).ToList();

            if (fromDate.HasValue)
                disbursements = disbursements.Where(d => d.Date >= fromDate.Value).ToList();

            if (toDate.HasValue)
                disbursements = disbursements.Where(d => d.Date <= toDate.Value).ToList();

            if (benefitId.HasValue)
                disbursements = disbursements.Where(d => d.BenefitID == benefitId.Value).ToList();

            if (officerId.HasValue)
                disbursements = disbursements.Where(d => d.OfficerID == officerId.Value).ToList();

            var result = disbursements
                .OrderByDescending(d => d.Date)
                .Select(d => new
                {
                    d.DisbursementID,
                    d.Amount,
                    d.Status,
                    d.Date,
                    d.CitizenID,
                    d.OfficerID,
                    BenefitID = d.BenefitID,
                    BenefitType = d.Benefit?.Type,
                    BenefitAmount = d.Benefit?.Amount,
                    BenefitStatus = d.Benefit?.Status,
                    Citizen = new
                    {
                        d.CitizenID
                    },
                    Benefit = new
                    {
                        d.Benefit?.BenefitID,
                        d.Benefit?.Type,
                        d.Benefit?.Amount,
                        d.Benefit?.Status,
                        Application = new
                        {
                            d.Benefit?.WelfareApplication?.ApplicationID,
                            d.Benefit?.WelfareApplication?.Status
                        }
                    }
                });

            return Ok(result);
        }

        // GET: api/disbursementapi/pending
        /// <summary>
        /// Get all pending disbursements
        /// </summary>
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingDisbursements()
        {
            return await GetFilteredDisbursements(status: "Pending");
        }

        // GET: api/disbursementapi/history
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? benefitType,
            [FromQuery] int? officerId,
            [FromQuery] string? status)
        {
            var disbursements = await _disbursementService.GetAllDisbursementsAsync();

            if (startDate.HasValue)
                disbursements = disbursements.Where(d => d.Date >= startDate.Value);
            if (endDate.HasValue)
                disbursements = disbursements.Where(d => d.Date <= endDate.Value);
            if (!string.IsNullOrEmpty(benefitType))
                disbursements = disbursements.Where(d => d.Benefit != null && d.Benefit.Type == benefitType);
            if (officerId.HasValue)
                disbursements = disbursements.Where(d => d.OfficerID == officerId.Value);
            if (!string.IsNullOrEmpty(status))
                disbursements = disbursements.Where(d => d.Status == status);

            return Ok(disbursements.ToList());
        }

        // GET: api/disbursementapi/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var disbursement = await _disbursementService.GetDisbursementByIdAsync(id);
            if (disbursement == null) return NotFound();

            var siblings = await _disbursementService.GetDisbursementsByBenefitIdAsync(disbursement.BenefitID);
            var totalDisbursed = siblings.Where(d => d.Status == "Completed").Sum(d => d.Amount);
            var pendingBalance = siblings
                .Where(d => d.Status == "Pending" || d.Status == "Disbursement Pending")
                .Sum(d => d.Amount);

            return Ok(new
            {
                Disbursement = disbursement,
                BenefitTotalAmount = disbursement.Benefit?.Amount ?? 0,
                TotalDisbursed = totalDisbursed,
                PendingBalance = pendingBalance,
                SiblingDisbursements = siblings.Where(d => d.DisbursementID != id).OrderBy(d => d.Date)
            });
        }

        // GET: api/disbursementapi/benefit/{benefitId}
        [HttpGet("benefit/{benefitId}")]
        public async Task<IActionResult> GetByBenefitId(int benefitId)
        {
            var disbursements = await _disbursementService.GetDisbursementsByBenefitIdAsync(benefitId);
            return Ok(disbursements);
        }

        // GET: api/disbursementapi/benefit-details/{benefitId}
        [HttpGet("benefit-details/{benefitId}")]
        public async Task<IActionResult> GetBenefitDetails(int benefitId)
        {
            if (benefitId <= 0) return BadRequest(new { Error = "Invalid benefit ID." });

            var benefit = await _benefitService.GetBenefitByIdAsync(benefitId);
            if (benefit == null) return NotFound();

            var programId = benefit.WelfareApplication?.ProgramID ?? 0;

            var resources = programId > 0
                ? await _resourceService.GetResourcesByProgramIdAsync(programId)
                : Enumerable.Empty<Resource>();
            var totalResource = (double)resources.Sum(r => r.Quantity);

            var allDisbursements = await _disbursementService.GetAllDisbursementsAsync();
            var totalDisbursedForProgram = allDisbursements
                .Where(d => d.Status == "Completed" && d.Benefit?.WelfareApplication?.ProgramID == programId)
                .Sum(d => d.Amount);

            var availableResource = totalResource - totalDisbursedForProgram;

            return Ok(new
            {
                benefitType = benefit.Type,
                benefitAmount = benefit.Amount,
                benefitStatus = benefit.Status,
                programTitle = benefit.WelfareApplication?.Program?.Title,
                programBudget = benefit.WelfareApplication?.Program?.Budget,
                citizenId = benefit.WelfareApplication?.CitizenID,
                citizenName = benefit.WelfareApplication?.Citizen?.Name,
                totalResource,
                totalDisbursedForProgram,
                availableResource,
                isResourceExhausted = totalResource > 0 && availableResource <= 0
            });
        }

        // POST: api/disbursementapi
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Disbursement disbursement)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var created = await _disbursementService.CreateDisbursementAsync(disbursement);
                return Ok(new { Message = $"Disbursement #{created.DisbursementID} created successfully.", Disbursement = created });
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        // PUT: api/disbursementapi/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Disbursement disbursement)
        {
            if (id != disbursement.DisbursementID) return BadRequest(new { Error = "ID mismatch." });
            if (!await _disbursementService.DisbursementExistsAsync(id)) return NotFound();

            try
            {
                var updated = await _disbursementService.UpdateDisbursementAsync(disbursement);
                return Ok(new { Message = $"Disbursement #{updated.DisbursementID} updated successfully.", Disbursement = updated });
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        // DELETE: api/disbursementapi/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var disbursement = await _disbursementService.GetDisbursementByIdAsync(id);
            if (disbursement == null) return NotFound();

            if (disbursement.Status == "Completed")
                return BadRequest(new { Error = "Cannot delete a completed disbursement." });

            await _disbursementService.DeleteDisbursementAsync(id);
            return Ok(new { Message = $"Disbursement #{id} deleted successfully." });
        }
    }
}
