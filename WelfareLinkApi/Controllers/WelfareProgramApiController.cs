using Microsoft.AspNetCore.Mvc;
using WelfareLinkApi.Interfaces;
using WelfareLinkApi.Models;
using WelfareLinkApi.ViewModels;

namespace WelfareLinkApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WelfareProgramApiController : ControllerBase
    {
        private readonly IWelfareProgramService _programService;
        private readonly IResourceService _resourceService;
        private readonly IDisbursementService _disbursementService;
        private readonly IWelfareApplicationService _applicationService;
        private readonly IBenefitService _benefitService;

        public WelfareProgramApiController(
            IWelfareProgramService programService,
            IResourceService resourceService,
            IDisbursementService disbursementService,
            IWelfareApplicationService applicationService,
            IBenefitService benefitService)
        {
            _programService = programService;
            _resourceService = resourceService;
            _disbursementService = disbursementService;
            _applicationService = applicationService;
            _benefitService = benefitService;
        }

        // GET: api/welfareprogramapi
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var programs = await _programService.GetAllProgramsAsync();
            return Ok(programs);
        }

        // GET: api/welfareprogramapi/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var program = await _programService.GetProgramByIdAsync(id);
            if (program == null) return NotFound();

            var resources = await _resourceService.GetResourcesByProgramIdAsync(id);
            var totalAllocatedFunds = resources
                .Where(r => r.Type.Equals("Funds", StringComparison.OrdinalIgnoreCase))
                .Sum(r => r.Quantity);
            var totalAllocatedMaterials = resources
                .Where(r => r.Type.Equals("Materials", StringComparison.OrdinalIgnoreCase))
                .Sum(r => r.Quantity);
            var utilisationPercentage = program.Budget > 0
                ? (totalAllocatedFunds / program.Budget) * 100
                : 0;

            return Ok(new ProgramDetailViewModel
            {
                Program = program,
                Resources = resources,
                TotalAllocatedFunds = totalAllocatedFunds,
                TotalAllocatedMaterials = totalAllocatedMaterials,
                UtilisationPercentage = utilisationPercentage,
                RemainingBudget = program.Budget - totalAllocatedFunds,
                IsBudgetCritical = utilisationPercentage >= 80
            });
        }

        // GET: api/welfareprogramapi/budget-monitoring
        [HttpGet("budget-monitoring")]
        public async Task<IActionResult> GetBudgetMonitoring()
        {
            var programs = await _programService.GetAllProgramsAsync();
            var allDisbursements = await _disbursementService.GetAllDisbursementsAsync();
            var budgetViewModels = new List<BudgetMonitoringViewModel>();

            foreach (var program in programs)
            {
                var resources = await _resourceService.GetResourcesByProgramIdAsync(program.ProgramID);
                var allocatedFunds = resources
                    .Where(r => r.Type.Equals("Funds", StringComparison.OrdinalIgnoreCase))
                    .Sum(r => r.Quantity);

                var disbursedFunds = (decimal)allDisbursements
                    .Where(d => d.Status == "Completed" && d.Benefit?.WelfareApplication?.ProgramID == program.ProgramID)
                    .Sum(d => d.Amount);

                var utilisation = program.Budget > 0 ? (allocatedFunds / program.Budget) * 100 : 0;

                budgetViewModels.Add(new BudgetMonitoringViewModel
                {
                    ProgramID = program.ProgramID,
                    ProgramTitle = program.Title,
                    TotalBudget = program.Budget,
                    AllocatedFunds = allocatedFunds,
                    DisbursedFunds = disbursedFunds,
                    RemainingBudget = program.Budget - allocatedFunds,
                    UtilisationPercentage = utilisation,
                    Status = program.Status,
                    IsCritical = utilisation >= 80
                });
            }

            return Ok(new BudgetDashboardViewModel
            {
                ProgramBudgets = budgetViewModels,
                TotalBudgetAllPrograms = budgetViewModels.Sum(b => b.TotalBudget),
                TotalAllocated = budgetViewModels.Sum(b => b.AllocatedFunds),
                TotalRemaining = budgetViewModels.Sum(b => b.RemainingBudget),
                CriticalProgramsCount = budgetViewModels.Count(b => b.IsCritical)
            });
        }

        // GET: api/welfareprogramapi/performance
        [HttpGet("performance")]
        public async Task<IActionResult> GetPerformance()
        {
            var programs = await _programService.GetAllProgramsAsync();
            var allApplications = await _applicationService.GetAllApplicationsAsync();
            var allDisbursements = await _disbursementService.GetAllDisbursementsAsync();

            var performanceList = new List<ProgramPerformanceViewModel>();
            foreach (var program in programs)
            {
                var programApps = allApplications
                    .Where(a => a.ProgramID == program.ProgramID)
                    .ToList();
                var approved = programApps.Count(a => a.Status == "Approved");
                var total = programApps.Count;

                var resources = await _resourceService.GetResourcesByProgramIdAsync(program.ProgramID);
                var allocatedFunds = resources
                    .Where(r => r.Type.Equals("Funds", StringComparison.OrdinalIgnoreCase))
                    .Sum(r => r.Quantity);

                performanceList.Add(new ProgramPerformanceViewModel
                {
                    ProgramID = program.ProgramID,
                    ProgramTitle = program.Title,
                    TotalApplications = total,
                    ApprovedApplications = approved,
                    RejectedApplications = programApps.Count(a => a.Status == "Rejected"),
                    PendingApplications = programApps.Count(a => a.Status == "Pending"),
                    ApprovalRate = total > 0 ? (decimal)approved / total * 100 : 0,
                    BenefitsDisbursed = allDisbursements
                        .Count(d => d.Status == "Completed" && d.Benefit?.WelfareApplication?.ProgramID == program.ProgramID),
                    CitizenCount = programApps.Select(a => a.CitizenID).Distinct().Count(),
                    BudgetUtilisation = program.Budget > 0 ? (allocatedFunds / program.Budget) * 100 : 0,
                    Status = program.Status
                });
            }

            return Ok(performanceList);
        }

        // POST: api/welfareprogramapi
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] WelfareProgram program)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                await _programService.AddProgramAsync(program);
                return Ok(new { Message = "Programme created successfully.", Program = program });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        // PUT: api/welfareprogramapi/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] WelfareProgram program)
        {
            if (id != program.ProgramID) return BadRequest(new { Error = "ID mismatch." });

            try
            {
                await _programService.UpdateProgramAsync(program);
                return Ok(new { Message = $"Programme #{id} updated successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        // PATCH: api/welfareprogramapi/{id}/suspend
        [HttpPatch("{id}/suspend")]
        public async Task<IActionResult> Suspend(int id)
        {
            var program = await _programService.GetProgramByIdAsync(id);
            if (program == null) return NotFound();

            try
            {
                await _programService.SuspendProgramAsync(id);
                return Ok(new { Message = $"Programme #{id} suspended successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}
