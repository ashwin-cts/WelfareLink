using Microsoft.AspNetCore.Mvc;
using WelfareLinkApi.Data;
using WelfareLinkApi.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace WelfareLinkApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditorDashboardApiController : ControllerBase
    {
        private readonly WelfareLinkDbContext _context;
        private readonly IAuditMonitoringService _auditMonitoringService;
        private readonly IAuditLogServiceEnhanced _auditLogService;

        public AuditorDashboardApiController(
            WelfareLinkDbContext context,
            IAuditMonitoringService auditMonitoringService,
            IAuditLogServiceEnhanced auditLogService)
        {
            _context = context;
            _auditMonitoringService = auditMonitoringService;
            _auditLogService = auditLogService;
        }

        /// <summary>
        /// Get program budget monitoring information
        /// </summary>
        [HttpGet("budget-monitoring")]
        public async Task<IActionResult> GetBudgetMonitoring()
        {
            var programs = await _context.Programs
                .Include(p => p.WelfareApplications)
                    .ThenInclude(a => a!.Benefits)
                .AsNoTracking()
                .Select(p => new
                {
                    p.ProgramID,
                    p.Title,
                    p.Description,
                    p.Budget,
                    p.Status,
                    p.StartDate,
                    p.EndDate,
                    p.MaxBenefitPerCitizen,
                    TotalAllocated = p.WelfareApplications!
                        .SelectMany(a => a.Benefits!)
                        .Where(b => b.Status != "Failed" && b.Status != "Cancelled")
                        .Sum(b => b.Amount),
                    BenefitsCount = p.WelfareApplications!
                        .SelectMany(a => a.Benefits!)
                        .Count(),
                    ApplicationsCount = p.WelfareApplications!.Count(),
                    BudgetUtilizationPercentage = p.Budget > 0 ? (p.WelfareApplications!
                        .SelectMany(a => a.Benefits!)
                        .Where(b => b.Status != "Failed" && b.Status != "Cancelled")
                        .Sum(b => (decimal)b.Amount) / p.Budget) * 100 : 0,
                    RemainingBudget = p.Budget - (p.WelfareApplications!
                        .SelectMany(a => a.Benefits!)
                        .Where(b => b.Status != "Failed" && b.Status != "Cancelled")
                        .Sum(b => (decimal)b.Amount))
                })
                .ToListAsync();

            return Ok(programs);
        }

        /// <summary>
        /// Get resource utilization for programs
        /// </summary>
        [HttpGet("resource-utilization")]
        public async Task<IActionResult> GetResourceUtilization()
        {
            var resources = await _context.Resources
                .Include(r => r.Program)
                    .ThenInclude(p => p!.WelfareApplications)
                        .ThenInclude(a => a!.Benefits)
                .AsNoTracking()
                .Select(r => new
                {
                    r.ResourceID,
                    r.Type,
                    r.Quantity,
                    r.Status,
                    Program = new
                    {
                        r.Program!.ProgramID,
                        r.Program.Title,
                        r.Program.Budget
                    },
                    AllocatedBenefits = r.Program!.WelfareApplications!
                        .SelectMany(a => a.Benefits!)
                        .Where(b => b.Status != "Failed" && b.Status != "Cancelled")
                        .Count(),
                    TotalBenefitAmount = r.Program!.WelfareApplications!
                        .SelectMany(a => a.Benefits!)
                        .Where(b => b.Status != "Failed" && b.Status != "Cancelled")
                        .Sum(b => b.Amount),
                    UtilizationStatus = r.Status
                })
                .ToListAsync();

            return Ok(resources);
        }

        /// <summary>
        /// Get comprehensive dashboard metrics
        /// </summary>
        [HttpGet("metrics")]
        public async Task<IActionResult> GetDashboardMetrics()
        {
            var totalPrograms = await _context.Programs.CountAsync();
            var activePrograms = await _context.Programs
                .Where(p => p.Status == "Active")
                .CountAsync();

            var totalApplications = await _context.WelfareApplications.CountAsync();
            var approvedApplications = await _context.WelfareApplications
                .Where(a => a.Status == "Approved")
                .CountAsync();

            var totalBenefits = await _context.Benefits.CountAsync();
            var totalBenefitAmount = await _context.Benefits
                .Where(b => b.Status != "Failed" && b.Status != "Cancelled")
                .SumAsync(b => b.Amount);

            var totalDisbursements = await _context.Disbursements.CountAsync();
            var totalDisbursedAmount = await _context.Disbursements
                .SumAsync(d => d.Amount);

            var totalComplianceIssues = await _context.ComplianceRecords
                .CountAsync(c => c.Status == "Open");

            var totalProgramBudget = await _context.Programs
                .SumAsync(p => p.Budget);

            var auditorMetrics = new
            {
                Programs = new { Total = totalPrograms, Active = activePrograms },
                Applications = new { Total = totalApplications, Approved = approvedApplications },
                Benefits = new { Total = totalBenefits, TotalAmount = totalBenefitAmount },
                Disbursements = new { Total = totalDisbursements, TotalAmount = totalDisbursedAmount },
                Compliance = new { OpenIssues = totalComplianceIssues },
                Budget = new { Total = totalProgramBudget, Allocated = totalBenefitAmount }
            };

            return Ok(auditorMetrics);
        }

        /// <summary>
        /// Get detailed benefit flow for a program
        /// </summary>
        [HttpGet("benefit-flow/{programID}")]
        public async Task<IActionResult> GetBenefitFlow(int programID)
        {
            var program = await _context.Programs
                .Include(p => p.WelfareApplications)
                    .ThenInclude(a => a!.Citizen)
                .Include(p => p.WelfareApplications)
                    .ThenInclude(a => a!.Benefits)
                        .ThenInclude(b => b!.Disbursements)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProgramID == programID);

            if (program == null)
                return NotFound(new { Error = "Program not found" });

            var benefitFlow = new
            {
                Program = new
                {
                    program.ProgramID,
                    program.Title,
                    program.Budget,
                    program.MaxBenefitPerCitizen,
                    program.Status
                },
                Applications = program.WelfareApplications?.Select(a => new
                {
                    a.ApplicationID,
                    a.Status,
                    a.SubmittedDate,
                    CitizenName = a.Citizen?.Name,
                    Benefits = a.Benefits?.Select(b => new
                    {
                        b.BenefitID,
                        b.Amount,
                        b.Status,
                        b.Date,
                        Disbursements = b.Disbursements?.Select(d => new
                        {
                            d.DisbursementID,
                            d.Amount,
                            d.Status,
                            d.Date
                        }).ToList()
                    }).ToList()
                }).ToList()
            };

            return Ok(benefitFlow);
        }

        /// <summary>
        /// Get AuditLog system logs
        /// </summary>
        [HttpGet("system-logs")]
        public async Task<IActionResult> GetSystemLogs(int pageNumber = 1, int pageSize = 50)
        {
            var logs = await _context.AuditLogs
                .Include(l => l.User)
                .AsNoTracking()
                .OrderByDescending(l => l.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new
                {
                    l.LogID,
                    l.Action,
                    l.EntityType,
                    l.EntityId,
                    l.Description,
                    l.Status,
                    l.Timestamp,
                    User = l.User != null ? new { l.User.UserId, l.User.Username } : null,
                    l.IPAddress,
                    ChangeSummary = l.OldValue != null ? $"Old: {l.OldValue}" : null
                })
                .ToListAsync();

            var totalLogs = await _context.AuditLogs.CountAsync();

            return Ok(new
            {
                Logs = logs,
                Pagination = new
                {
                    TotalRecords = totalLogs,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalLogs / pageSize)
                }
            });
        }

        /// <summary>
        /// Get user activity history
        /// </summary>
        [HttpGet("user-activity/{userID}")]
        public async Task<IActionResult> GetUserActivity(int userID)
        {
            var activities = await _context.AuditLogs
                .Where(l => l.UserId == userID)
                .AsNoTracking()
                .OrderByDescending(l => l.Timestamp)
                .Select(l => new
                {
                    l.LogID,
                    l.Action,
                    l.EntityType,
                    l.EntityId,
                    l.Description,
                    l.Timestamp,
                    l.IPAddress
                })
                .Take(100)
                .ToListAsync();

            return Ok(activities);
        }

        /// <summary>
        /// Get all entity changes/audit trail for compliance
        /// </summary>
        [HttpGet("entity-changes/{entityType}/{entityID}")]
        public async Task<IActionResult> GetEntityChanges(string entityType, int entityID)
        {
            var changes = await _context.AuditLogs
                .Where(l => l.EntityType == entityType && l.EntityId == entityID)
                .Include(l => l.User)
                .AsNoTracking()
                .OrderByDescending(l => l.Timestamp)
                .Select(l => new
                {
                    l.LogID,
                    l.Action,
                    l.Description,
                    l.OldValue,
                    l.NewValue,
                    l.Timestamp,
                    User = l.User != null ? new { l.User.Username } : null
                })
                .ToListAsync();

            return Ok(changes);
        }

        /// <summary>
        /// Get comprehensive program audit report with enhanced monitoring
        /// </summary>
        [HttpGet("program-audit-report/{programID}")]
        public async Task<IActionResult> GetProgramAuditReport(int programID)
        {
            try
            {
                var report = await _auditMonitoringService.GetProgramAuditReportAsync(programID);
                return Ok(new { success = true, data = report });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get comprehensive budget tracking across all programs
        /// </summary>
        [HttpGet("budget-tracking-enhanced")]
        public async Task<IActionResult> GetBudgetTrackingEnhanced()
        {
            try
            {
                var budgetReports = await _auditMonitoringService.GetComprehensiveBudgetTrackingAsync();
                var summary = new
                {
                    TotalPrograms = budgetReports.Count,
                    TotalBudgetAllocated = budgetReports.Sum(r => r.TotalBudget),
                    TotalAllocatedBenefits = budgetReports.Sum(r => r.AllocatedBenefits),
                    TotalDisbursed = budgetReports.Sum(r => r.DisbursedAmount),
                    TotalRemaining = budgetReports.Sum(r => r.RemainingBudget),
                    AvgUtilizationPercent = budgetReports.Count > 0 ? budgetReports.Average(r => r.BudgetUtilizationPercentage) : 0,
                    Reports = budgetReports
                };

                return Ok(new { success = true, data = summary });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get money flow analysis for a program
        /// </summary>
        [HttpGet("money-flow/{programID}")]
        public async Task<IActionResult> GetMoneyFlowAnalysis(int programID)
        {
            try
            {
                var analysis = await _auditMonitoringService.GetMoneyFlowAnalysisAsync(programID);
                return Ok(new { success = true, data = analysis });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get resource allocation summary for a program
        /// </summary>
        [HttpGet("resource-allocation-enhanced/{programID}")]
        public async Task<IActionResult> GetResourceAllocationSummary(int programID)
        {
            try
            {
                var resources = await _auditMonitoringService.GetResourceAllocationSummaryAsync(programID);
                return Ok(new { success = true, data = resources, count = resources.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get pending resources requiring approval
        /// </summary>
        [HttpGet("pending-resources")]
        public async Task<IActionResult> GetPendingResources([FromQuery] int? programID = null)
        {
            try
            {
                var resources = await _auditMonitoringService.GetPendingResourcesAsync(programID);

                var result = resources.Select(r => new
                {
                    r.ResourceID,
                    r.ProgramID,
                    ProgramName = r.Program?.Title ?? "Unknown",
                    r.Type,
                    r.Quantity,
                    r.Status
                }).ToList();

                return Ok(new { success = true, data = result, count = result.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Approve a pending resource
        /// </summary>
        [HttpPost("approve-resource/{resourceID}")]
        public async Task<IActionResult> ApproveResource(int resourceID, [FromBody] ApproveResourceRequest request)
        {
            try
            {
                int auditorID = HttpContext.Session.GetInt32("UserId") ?? 0;

                await _auditMonitoringService.ApproveResourceAsync(resourceID, auditorID, request.Notes);

                return Ok(new { success = true, message = "Resource approved successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Flag a resource as insufficient
        /// </summary>
        [HttpPost("flag-resource/{resourceID}")]
        public async Task<IActionResult> FlagResourceAsInsufficient(int resourceID, [FromBody] FlagResourceRequest request)
        {
            try
            {
                int auditorID = HttpContext.Session.GetInt32("UserId") ?? 0;

                await _auditMonitoringService.FlagResourceAsInsufficientAsync(resourceID, auditorID, request.Reason);

                return Ok(new { success = true, message = "Resource flagged as insufficient" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get open audit findings
        /// </summary>
        [HttpGet("open-audit-findings")]
        public async Task<IActionResult> GetOpenAuditFindings([FromQuery] int? programID = null)
        {
            try
            {
                var findings = await _auditMonitoringService.GetOpenAuditFindingsAsync(programID);

                var result = findings.Select(f => new
                {
                    f.AuditID,
                    f.ProgramID,
                    ProgramName = f.WelfareProgram?.Title ?? "Unknown",
                    f.FindingType,
                    f.Description,
                    f.Status,
                    f.AuditDate,
                    AuditedBy = f.AuditedByUser?.Username ?? "Unknown",
                    DaysSinceAudit = (DateTime.UtcNow - f.AuditDate).Days
                }).ToList();

                return Ok(new { success = true, data = result, count = result.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Close an audit finding
        /// </summary>
        [HttpPost("close-audit-finding/{auditID}")]
        public async Task<IActionResult> CloseAuditFinding(int auditID, [FromBody] CloseAuditFindingRequest request)
        {
            try
            {
                await _auditMonitoringService.CloseAuditFindingAsync(auditID, request.ResolutionNotes);

                return Ok(new { success = true, message = "Audit finding closed" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get audit trail for a program
        /// </summary>
        [HttpGet("program-audit-trail/{programID}")]
        public async Task<IActionResult> GetProgramAuditTrail(
            int programID,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            try
            {
                var trail = await _auditMonitoringService.GetProgramAuditTrailAsync(programID, from, to);

                var result = trail.Select(t => new
                {
                    t.LogID,
                    t.Action,
                    t.EntityType,
                    t.EntityId,
                    t.Description,
                    t.Status,
                    t.Timestamp,
                    PerformedBy = t.User?.Username ?? "System",
                    Changes = new { OldValue = t.OldValue, NewValue = t.NewValue }
                }).ToList();

                return Ok(new { success = true, data = result, count = result.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get activity summary for audit period
        /// </summary>
        [HttpGet("activity-summary")]
        public async Task<IActionResult> GetActivitySummary(
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            try
            {
                var startDate = from ?? DateTime.UtcNow.AddDays(-30);
                var endDate = to ?? DateTime.UtcNow;

                var activitySummary = await _auditLogService.GetActivitySummaryAsync(startDate, endDate);

                return Ok(new { success = true, data = activitySummary });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get comprehensive auditor dashboard summary
        /// </summary>
        [HttpGet("dashboard-summary-enhanced")]
        public async Task<IActionResult> GetAuditorDashboardSummaryEnhanced()
        {
            try
            {
                var budgetReports = await _auditMonitoringService.GetComprehensiveBudgetTrackingAsync();
                var openFindings = await _auditMonitoringService.GetOpenAuditFindingsAsync();
                var pendingResources = await _auditMonitoringService.GetPendingResourcesAsync();

                // Calculate key metrics
                var highUtilizationPrograms = budgetReports.Where(r => r.BudgetUtilizationPercentage > 90).Count();
                var lowUtilizationPrograms = budgetReports.Where(r => r.BudgetUtilizationPercentage < 30).Count();

                var summary = new
                {
                    TotalPrograms = budgetReports.Count,
                    TotalBudget = budgetReports.Sum(r => r.TotalBudget),
                    AllocatedBenefits = budgetReports.Sum(r => r.AllocatedBenefits),
                    DisbursedAmount = budgetReports.Sum(r => r.DisbursedAmount),
                    RemainingBudget = budgetReports.Sum(r => r.RemainingBudget),
                    AverageBudgetUtilization = budgetReports.Count > 0 ? budgetReports.Average(r => r.BudgetUtilizationPercentage) : 0,
                    HighUtilizationPrograms = highUtilizationPrograms,
                    LowUtilizationPrograms = lowUtilizationPrograms,
                    OpenAuditFindings = openFindings.Count,
                    InsufficientResourceFlags = openFindings.Count(f => f.FindingType == "InsufficientResource"),
                    PendingResourceApprovals = pendingResources.Count,
                    Timestamp = DateTime.UtcNow
                };

                return Ok(new { success = true, data = summary });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get programs overview with health status
        /// </summary>
        [HttpGet("programs-overview")]
        public async Task<IActionResult> GetProgramsOverview()
        {
            try
            {
                var budgetReports = await _auditMonitoringService.GetComprehensiveBudgetTrackingAsync();

                var result = budgetReports.Select(r => new
                {
                    r.ProgramID,
                    r.ProgramTitle,
                    r.TotalBudget,
                    r.AllocatedBenefits,
                    r.DisbursedAmount,
                    r.RemainingBudget,
                    r.BudgetUtilizationPercentage,
                    r.Status,
                    HealthStatus = r.BudgetUtilizationPercentage > 90 ? "High" :
                                  r.BudgetUtilizationPercentage > 70 ? "Medium" :
                                  r.BudgetUtilizationPercentage > 30 ? "Low" : "Very Low"
                }).ToList();

                return Ok(new { success = true, data = result, count = result.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }

    // Request DTOs
    public class ApproveResourceRequest
    {
        public string? Notes { get; set; }
    }

    public class FlagResourceRequest
    {
        public string Reason { get; set; } = string.Empty;
    }

    public class CloseAuditFindingRequest
    {
        public string ResolutionNotes { get; set; } = string.Empty;
    }
}
