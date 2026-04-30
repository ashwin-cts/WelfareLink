using Microsoft.AspNetCore.Mvc;
using WelfareLink.AuditorManagement.API.Interfaces;
using WelfareLink.AuditorManagement.API.Models;
using WelfareLink.AuditorManagement.API.ViewModels;
using WelfareLink.AuditorManagement.API.Utilities;

namespace WelfareLink.AuditorManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GovernmentAuditorApiController : ControllerBase
    {
        private readonly IBenefitAnalyticsService _benefitAnalyticsService;
        private readonly IAuditLogService _auditLogService;
        private readonly IWelfareApplicationRepository _applicationRepository;
        private readonly IWelfareProgramRepository _programRepository;
        private readonly IResourceRepository _resourceRepository;
        private readonly IDisbursementRepository _disbursementRepository;
        private readonly IBenefitRepository _benefitRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GovernmentAuditorApiController(
            IBenefitAnalyticsService benefitAnalyticsService,
            IAuditLogService auditLogService,
            IWelfareApplicationRepository applicationRepository,
            IWelfareProgramRepository programRepository,
            IResourceRepository resourceRepository,
            IDisbursementRepository disbursementRepository,
            IBenefitRepository benefitRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _benefitAnalyticsService = benefitAnalyticsService;
            _auditLogService = auditLogService;
            _applicationRepository = applicationRepository;
            _programRepository = programRepository;
            _resourceRepository = resourceRepository;
            _disbursementRepository = disbursementRepository;
            _benefitRepository = benefitRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Get Dashboard - Displays summary metrics
        /// Total Applications, Total Programs, Total Budget, Total Resource, Total Disbursement
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<ActionResult<dynamic>> GetDashboard()
        {
            var ipAddress = AuditLogHelper.GetClientIpAddress(HttpContext);
            var userAgent = AuditLogHelper.GetUserAgent(HttpContext);
            var currentUserId = GetUserIdFromClaims();

            try
            {
                // Get analytics dashboard data
                var dashboardData = await _benefitAnalyticsService.GetDashboardDataAsync();

                // Get all applications
                var applications = await _applicationRepository.GetAllAsync();
                var totalApplications = applications.Count();

                // Get all programs
                var programs = await _programRepository.GetAllProgramsAsync();
                var totalPrograms = programs.Count();

                // Calculate total budget
                decimal totalBudget = programs.Sum(p => p.Budget);

                // Get all resources
                var resources = await _resourceRepository.GetAllResourcesAsync();
                decimal totalResource = resources.Sum(r => r.Quantity);

                // Get all disbursements
                var disbursements = await _disbursementRepository.GetAllAsync();
                decimal totalDisbursement = disbursements.Sum(d => (decimal)d.Amount);

                dynamic result = new System.Dynamic.ExpandoObject();
                var dict = (IDictionary<string, object>)result;
                dict["TotalApplications"] = totalApplications;
                dict["TotalPrograms"] = totalPrograms;
                dict["TotalBudget"] = totalBudget;
                dict["TotalResource"] = totalResource;
                dict["TotalDisbursement"] = totalDisbursement;
                dict["AnalyticsDashboard"] = dashboardData;

                // Log the action
                await _auditLogService.LogActionAsync(
                    userId: currentUserId,
                    action: "Read",
                    entityType: "Dashboard",
                    entityId: null,
                    description: $"Government Auditor accessed dashboard",
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    status: "Success"
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                await _auditLogService.LogActionAsync(
                    userId: currentUserId,
                    action: "Read",
                    entityType: "Dashboard",
                    entityId: null,
                    description: $"Error accessing dashboard: {ex.Message}",
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    status: "Failed"
                );

                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get Budget Monitoring - Program Breakdown
        /// Shows all programs with budget details, resources, citizens, disbursements
        /// </summary>
        [HttpGet("budget-monitoring")]
        public async Task<ActionResult<List<dynamic>>> GetBudgetMonitoring()
        {
            var ipAddress = AuditLogHelper.GetClientIpAddress(HttpContext);
            var userAgent = AuditLogHelper.GetUserAgent(HttpContext);
            var currentUserId = GetUserIdFromClaims();

            try
            {
                var programs = await _programRepository.GetAllProgramsAsync();
                var applications = await _applicationRepository.GetAllAsync();
                var benefits = await _benefitRepository.GetAllAsync();
                var disbursements = await _disbursementRepository.GetAllAsync();
                var resources = await _resourceRepository.GetAllResourcesAsync();

                var programBreakdown = new List<dynamic>();

                foreach (var program in programs)
                {
                    // Count citizens applied for this program
                    var citizensForProgram = applications
                        .Where(a => a.ProgramID == program.ProgramID)
                        .Select(a => a.CitizenID)
                        .Distinct()
                        .Count();

                    // Calculate total resources allocated to this program
                    decimal totalResourceAllocated = resources
                        .Where(r => r.ProgramID == program.ProgramID)
                        .Sum(r => r.Quantity);

                    // Calculate total disbursed for this program
                    decimal totalDisbursed = 0;

                    var programApplications = applications.Where(a => a.ProgramID == program.ProgramID);

                    foreach (var app in programApplications)
                    {
                        var appBenefits = benefits.Where(b => b.ApplicationID == app.ApplicationID);

                        foreach (var benefit in appBenefits)
                        {
                            var benefitDisbursements = disbursements.Where(d => d.BenefitID == benefit.BenefitID);
                            totalDisbursed += (decimal)benefitDisbursements.Sum(d => d.Amount);
                        }
                    }

                    decimal remaining = program.Budget - totalResourceAllocated;
                    decimal utilizationPercent = program.Budget > 0 ? (totalResourceAllocated / program.Budget) * 100 : 0;

                    dynamic item = new System.Dynamic.ExpandoObject();
                    var itemDict = (IDictionary<string, object>)item;
                    itemDict["ProgramID"] = program.ProgramID;
                    itemDict["ProgramName"] = program.Title;
                    itemDict["ProgramStatus"] = program.Status;
                    itemDict["ProgramBudget"] = program.Budget;
                    itemDict["AllocatedResource"] = totalResourceAllocated;
                    itemDict["CitizensApplied"] = citizensForProgram;
                    itemDict["TotalDisbursed"] = totalDisbursed;
                    itemDict["RemainingResource"] = remaining;
                    itemDict["UtilizationPercent"] = Math.Round(utilizationPercent, 2);

                    programBreakdown.Add(item);
                }

                // Log the action
                await _auditLogService.LogActionAsync(
                    userId: currentUserId,
                    action: "Read",
                    entityType: "BudgetMonitoring",
                    entityId: null,
                    description: $"Government Auditor accessed budget monitoring report (Programs: {programBreakdown.Count})",
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    status: "Success"
                );

                return Ok(programBreakdown);
            }
            catch (Exception ex)
            {
                await _auditLogService.LogActionAsync(
                    userId: currentUserId,
                    action: "Read",
                    entityType: "BudgetMonitoring",
                    entityId: null,
                    description: $"Error accessing budget monitoring: {ex.Message}",
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    status: "Failed"
                );

                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get Resource Allocation Statement
        /// Shows resource allocation history from Program Officer
        /// </summary>
        [HttpGet("resource-statement")]
        public async Task<ActionResult<List<dynamic>>> GetResourceStatement()
        {
            var ipAddress = AuditLogHelper.GetClientIpAddress(HttpContext);
            var userAgent = AuditLogHelper.GetUserAgent(HttpContext);
            var currentUserId = GetUserIdFromClaims();

            try
            {
                var resources = await _resourceRepository.GetAllResourcesAsync();
                var programs = await _programRepository.GetAllProgramsAsync();

                var resourceStatements = new List<dynamic>();

                foreach (var resource in resources)
                {
                    var program = programs.FirstOrDefault(p => p.ProgramID == resource.ProgramID);

                    // Calculate total resources allocated to this program
                    decimal totalResourcesAllocated = resources
                        .Where(r => r.ProgramID == resource.ProgramID)
                        .Sum(r => r.Quantity);

                    decimal programBudget = program?.Budget ?? 0;
                    decimal remainingAllocation = programBudget - totalResourcesAllocated;

                    dynamic item = new System.Dynamic.ExpandoObject();
                    var itemDict = (IDictionary<string, object>)item;
                    itemDict["ResourceID"] = resource.ResourceID;
                    itemDict["Date"] = DateTime.UtcNow;
                    itemDict["ProgramName"] = program?.Title ?? "Unknown";
                    itemDict["AllocatedResource"] = resource.Quantity;
                    itemDict["RemainingAllocationPending"] = Math.Max(remainingAllocation, 0);

                    resourceStatements.Add(item);
                }

                // Log the action
                await _auditLogService.LogActionAsync(
                    userId: currentUserId,
                    action: "Read",
                    entityType: "ResourceStatement",
                    entityId: null,
                    description: $"Government Auditor accessed resource allocation statement (Records: {resourceStatements.Count})",
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    status: "Success"
                );

                return Ok(resourceStatements);
            }
            catch (Exception ex)
            {
                await _auditLogService.LogActionAsync(
                    userId: currentUserId,
                    action: "Read",
                    entityType: "ResourceStatement",
                    entityId: null,
                    description: $"Error accessing resource statement: {ex.Message}",
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    status: "Failed"
                );

                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get Disbursement Statement
        /// Shows disbursement history with optional filters for Date and Citizen ID
        /// </summary>
        [HttpGet("disbursement-statement")]
        public async Task<ActionResult<List<dynamic>>> GetDisbursementStatement(
            [FromQuery] DateTime? filterDate = null,
            [FromQuery] int? filterCitizenId = null)
        {
            var ipAddress = AuditLogHelper.GetClientIpAddress(HttpContext);
            var userAgent = AuditLogHelper.GetUserAgent(HttpContext);
            var currentUserId = GetUserIdFromClaims();

            try
            {
                var applications = await _applicationRepository.GetAllAsync();
                var benefits = await _benefitRepository.GetAllAsync();
                var disbursements = await _disbursementRepository.GetAllAsync();
                var programs = await _programRepository.GetAllProgramsAsync();

                var disbursementStatements = new List<dynamic>();

                // Apply citizen filter
                var filteredApplications = applications.AsEnumerable();
                if (filterCitizenId.HasValue)
                {
                    filteredApplications = filteredApplications.Where(a => a.CitizenID == filterCitizenId.Value);
                }

                foreach (var app in filteredApplications)
                {
                    var appBenefits = benefits.Where(b => b.ApplicationID == app.ApplicationID);

                    foreach (var benefit in appBenefits)
                    {
                        var appDisbursements = disbursements.Where(d => d.BenefitID == benefit.BenefitID);

                        // Apply date filter if provided
                        if (filterDate.HasValue)
                        {
                            appDisbursements = appDisbursements.Where(d => d.Date.Date == filterDate.Value.Date);
                        }

                        var program = programs.FirstOrDefault(p => p.ProgramID == app.ProgramID);

                        // If no disbursements for this benefit, still show it
                        if (!appDisbursements.Any())
                        {
                            dynamic item = new System.Dynamic.ExpandoObject();
                            var itemDict = (IDictionary<string, object>)item;
                            itemDict["CitizenID"] = app.CitizenID;
                            itemDict["CitizenName"] = app.Citizen?.Name ?? "Unknown";
                            itemDict["MaxBenefit"] = program?.MaxBenefitPerCitizen ?? 0;
                            itemDict["BenefitAllocated"] = (decimal)benefit.Amount;
                            itemDict["Disbursed"] = 0m;
                            itemDict["RemainDisburse"] = (decimal)benefit.Amount;
                            itemDict["DisbursementPercent"] = 0m;
                            disbursementStatements.Add(item);
                        }
                        else
                        {
                            decimal totalDisbursedForBenefit = (decimal)appDisbursements.Sum(d => d.Amount);
                            decimal remaining = (decimal)benefit.Amount - totalDisbursedForBenefit;
                            decimal disbursementPercent = benefit.Amount > 0 ? (totalDisbursedForBenefit / (decimal)benefit.Amount) * 100 : 0;

                            dynamic item = new System.Dynamic.ExpandoObject();
                            var itemDict = (IDictionary<string, object>)item;
                            itemDict["CitizenID"] = app.CitizenID;
                            itemDict["CitizenName"] = app.Citizen?.Name ?? "Unknown";
                            itemDict["MaxBenefit"] = program?.MaxBenefitPerCitizen ?? 0;
                            itemDict["BenefitAllocated"] = (decimal)benefit.Amount;
                            itemDict["Disbursed"] = totalDisbursedForBenefit;
                            itemDict["RemainDisburse"] = Math.Max(remaining, 0);
                            itemDict["DisbursementPercent"] = Math.Round(disbursementPercent, 2);
                            disbursementStatements.Add(item);
                        }
                    }
                }

                // Log the action with filter information
                var filterDescription = $"Government Auditor accessed disbursement statement";
                if (filterDate.HasValue)
                    filterDescription += $" (Date: {filterDate:yyyy-MM-dd})";
                if (filterCitizenId.HasValue)
                    filterDescription += $" (CitizenID: {filterCitizenId})";
                filterDescription += $" (Records: {disbursementStatements.Count})";

                await _auditLogService.LogActionAsync(
                    userId: currentUserId,
                    action: "Read",
                    entityType: "DisbursementStatement",
                    entityId: null,
                    description: filterDescription,
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    status: "Success"
                );

                return Ok(disbursementStatements);
            }
            catch (Exception ex)
            {
                await _auditLogService.LogActionAsync(
                    userId: currentUserId,
                    action: "Read",
                    entityType: "DisbursementStatement",
                    entityId: null,
                    description: $"Error accessing disbursement statement: {ex.Message}",
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    status: "Failed"
                );

                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get Audit Logs with pagination
        /// </summary>
        [HttpGet("audit-logs")]
        public async Task<ActionResult<dynamic>> GetAuditLogs(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var ipAddress = AuditLogHelper.GetClientIpAddress(HttpContext);
            var userAgent = AuditLogHelper.GetUserAgent(HttpContext);
            var currentUserId = GetUserIdFromClaims();

            try
            {
                var (logs, totalCount) = await _auditLogService.GetPagedAuditLogsAsync(pageNumber, pageSize);

                dynamic result = new System.Dynamic.ExpandoObject();
                var dict = (IDictionary<string, object>)result;
                dict["Logs"] = logs;
                dict["TotalCount"] = totalCount;
                dict["PageNumber"] = pageNumber;
                dict["PageSize"] = pageSize;
                dict["TotalPages"] = (int)Math.Ceiling((double)totalCount / pageSize);

                // Log the action
                await _auditLogService.LogActionAsync(
                    userId: currentUserId,
                    action: "Read",
                    entityType: "AuditLog",
                    entityId: null,
                    description: $"Government Auditor accessed audit logs (Page: {pageNumber}, Size: {pageSize})",
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    status: "Success"
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                await _auditLogService.LogActionAsync(
                    userId: currentUserId,
                    action: "Read",
                    entityType: "AuditLog",
                    entityId: null,
                    description: $"Error accessing audit logs: {ex.Message}",
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    status: "Failed"
                );

                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get Audit Logs by Date Range
        /// </summary>
        [HttpGet("audit-logs/date-range")]
        public async Task<ActionResult<dynamic>> GetAuditLogsByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var ipAddress = AuditLogHelper.GetClientIpAddress(HttpContext);
            var userAgent = AuditLogHelper.GetUserAgent(HttpContext);
            var currentUserId = GetUserIdFromClaims();

            try
            {
                var (logs, totalCount) = await _auditLogService.GetPagedAuditLogsByDateRangeAsync(startDate, endDate, pageNumber, pageSize);

                dynamic result = new System.Dynamic.ExpandoObject();
                var dict = (IDictionary<string, object>)result;
                dict["Logs"] = logs;
                dict["TotalCount"] = totalCount;
                dict["PageNumber"] = pageNumber;
                dict["PageSize"] = pageSize;
                dict["TotalPages"] = (int)Math.Ceiling((double)totalCount / pageSize);
                dict["StartDate"] = startDate;
                dict["EndDate"] = endDate;

                // Log the action
                await _auditLogService.LogActionAsync(
                    userId: currentUserId,
                    action: "Read",
                    entityType: "AuditLog",
                    entityId: null,
                    description: $"Government Auditor accessed audit logs for date range {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    status: "Success"
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                await _auditLogService.LogActionAsync(
                    userId: currentUserId,
                    action: "Read",
                    entityType: "AuditLog",
                    entityId: null,
                    description: $"Error accessing audit logs by date range: {ex.Message}",
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    status: "Failed"
                );

                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get Audit Logs by Entity Type
        /// </summary>
        [HttpGet("audit-logs/entity-type/{entityType}")]
        public async Task<ActionResult<dynamic>> GetAuditLogsByEntityType(
            string entityType,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var ipAddress = AuditLogHelper.GetClientIpAddress(HttpContext);
            var userAgent = AuditLogHelper.GetUserAgent(HttpContext);
            var currentUserId = GetUserIdFromClaims();

            try
            {
                var (logs, totalCount) = await _auditLogService.GetPagedAuditLogsByEntityTypeAsync(entityType, pageNumber, pageSize);

                dynamic result = new System.Dynamic.ExpandoObject();
                var dict = (IDictionary<string, object>)result;
                dict["Logs"] = logs;
                dict["TotalCount"] = totalCount;
                dict["PageNumber"] = pageNumber;
                dict["PageSize"] = pageSize;
                dict["TotalPages"] = (int)Math.Ceiling((double)totalCount / pageSize);
                dict["EntityType"] = entityType;

                // Log the action
                await _auditLogService.LogActionAsync(
                    userId: currentUserId,
                    action: "Read",
                    entityType: "AuditLog",
                    entityId: null,
                    description: $"Government Auditor accessed audit logs for entity type: {entityType}",
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    status: "Success"
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                await _auditLogService.LogActionAsync(
                    userId: currentUserId,
                    action: "Read",
                    entityType: "AuditLog",
                    entityId: null,
                    description: $"Error accessing audit logs by entity type: {ex.Message}",
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    status: "Failed"
                );

                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Extract User ID from JWT claims
        /// </summary>
        private int? GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst("UserId") ?? User.FindFirst("sub") ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            return null;
        }
    }
}
