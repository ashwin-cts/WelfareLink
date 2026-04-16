using Microsoft.AspNetCore.Mvc;
using WelfareLinkApi.Interfaces;
using WelfareLinkApi.Models;
using WelfareLinkApi.ViewModels;

namespace WelfareLinkApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResourceApiController : ControllerBase
    {
        private readonly IResourceService _resourceService;
        private readonly IWelfareProgramService _programService;
        private readonly IDisbursementService _disbursementService;

        public ResourceApiController(
            IResourceService resourceService,
            IWelfareProgramService programService,
            IDisbursementService disbursementService)
        {
            _resourceService = resourceService;
            _programService = programService;
            _disbursementService = disbursementService;
        }

        // GET: api/resourceapi
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var resources = await _resourceService.GetAllResourcesAsync();
            return Ok(resources);
        }

        // GET: api/resourceapi/program/{programId}
        [HttpGet("program/{programId}")]
        public async Task<IActionResult> GetByProgramId(int programId)
        {
            var program = await _programService.GetProgramByIdAsync(programId);
            if (program == null) return NotFound(new { Error = "Program not found." });

            var resources = await _resourceService.GetResourcesByProgramIdAsync(programId);
            var totalAllocated = resources
                .Where(r => r.Type.Equals("Funds", StringComparison.OrdinalIgnoreCase))
                .Sum(r => r.Quantity);

            // Map resources to DTOs with correct property names for the view
            var resourceDtos = resources.Select(r => new
            {
                ResourceID = r.ResourceID,
                Name = r.Type,  // Map Type to Name
                AmountAllocated = r.Quantity  // Map Quantity to AmountAllocated
            }).ToList();

            return Ok(new
            {
                Resources = resourceDtos,
                ProgramTitle = program.Title,
                ProgramBudget = program.Budget,
                TotalAllocated = totalAllocated,
                RemainingBudget = program.Budget - totalAllocated,
                UtilisationPercentage = program.Budget > 0 ? (totalAllocated / program.Budget) * 100 : 0
            });
        }

        // GET: api/resourceapi/utilisation
        [HttpGet("utilisation")]
        public async Task<IActionResult> GetUtilisationReport()
        {
            var resources = await _resourceService.GetAllResourcesAsync();
            var allDisbursements = await _disbursementService.GetAllDisbursementsAsync();
            var utilisationViewModels = new List<ResourceUtilisationViewModel>();

            foreach (var resource in resources)
            {
                var programBudget = resource.Program?.Budget ?? 0m;
                decimal usedQuantity;
                decimal totalDisbursed = 0m;
                decimal utilisationPercentage;

                if (resource.Type.Equals("Funds", StringComparison.OrdinalIgnoreCase))
                {
                    totalDisbursed = allDisbursements
                        .Where(d => d.Status == "Completed" && d.Benefit?.WelfareApplication?.ProgramID == resource.ProgramID)
                        .Sum(d => (decimal)d.Amount);

                    usedQuantity = Math.Min(totalDisbursed, resource.Quantity);
                    utilisationPercentage = resource.Quantity > 0
                        ? Math.Min((totalDisbursed / resource.Quantity) * 100, 100)
                        : 0;
                }
                else
                {
                    usedQuantity = resource.Status switch
                    {
                        "Depleted" => resource.Quantity,
                        "Reserved" => resource.Quantity * 0.5m,
                        _ => 0m
                    };
                    utilisationPercentage = resource.Quantity > 0
                        ? (usedQuantity / resource.Quantity) * 100
                        : 0;
                }

                utilisationViewModels.Add(new ResourceUtilisationViewModel
                {
                    ResourceID = resource.ResourceID,
                    Type = resource.Type,
                    InitialQuantity = resource.Quantity,
                    UsedQuantity = usedQuantity,
                    RemainingQuantity = resource.Quantity - usedQuantity,
                    ProgramBudget = programBudget,
                    TotalDisbursed = totalDisbursed,
                    Status = resource.Status,
                    ProgramID = resource.ProgramID,
                    ProgramTitle = resource.Program?.Title ?? "Unknown",
                    UtilisationPercentage = utilisationPercentage
                });
            }

            return Ok(utilisationViewModels);
        }

        // POST: api/resourceapi
        [HttpPost]
        public async Task<IActionResult> Allocate([FromBody] Resource resource)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                await _resourceService.AddResourceAsync(resource);
                return Ok(new { Message = "Resource allocated successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        // PUT: api/resourceapi/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Resource resource)
        {
            if (id != resource.ResourceID) return BadRequest(new { Error = "ID mismatch." });

            try
            {
                await _resourceService.UpdateResourceAsync(resource);
                return Ok(new { Message = $"Resource #{id} updated successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}
