using Microsoft.AspNetCore.Authorization; // ADDED FOR AUTHORIZATION
using System.Security.Claims; // ADDED FOR JWT CLAIM EXTRACTION
using Microsoft.AspNetCore.Mvc;
using WelfareLink.ComplianceAndAuditLog.API.Interfaces;
using WelfareLink.ComplianceAndAuditLog.API.Models;
using WelfareLink.ComplianceAndAuditLog.API.Data;
using Microsoft.EntityFrameworkCore;

namespace WelfareLink.ComplianceAndAuditLog.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // Base Rule: Only Admins and Compliance Officers can access this dashboard and its actions
    [Authorize(Roles = "Admin,ComplianceOfficer")]
    public class ComplianceOfficerDashboardApiController : ControllerBase
    {
        private readonly WelfareLinkDbContext _context;
        private readonly IComplianceCheckService _complianceService;

        public ComplianceOfficerDashboardApiController(
            WelfareLinkDbContext context,
            IComplianceCheckService complianceService)
        {
            _context = context;
            _complianceService = complianceService;
        }

        // Helper method to extract UserId from JWT token
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("UserId")?.Value;
            return int.TryParse(userIdClaim, out int id) ? id : 0;
        }

        /// <summary>
        /// Get all applications with benefit allocation and program details
        /// </summary>
        [HttpGet("applications")]
        public async Task<IActionResult> GetAllApplicationsWithBenefits()
        {
            var applications = await _context.WelfareApplications
                .Include(a => a.Citizen)
                .Include(a => a.Program)
                .Include(a => a.EligibilityChecks)
                .AsNoTracking()
                .Select(a => new
                {
                    a.ApplicationID,
                    a.Status,
                    a.SubmittedDate,
                    Citizen = new { a.Citizen!.CitizenId, a.Citizen.Name, a.Citizen.ContactInfo },
                    Program = new
                    {
                        a.Program!.ProgramID,
                        a.Program.Title,
                        a.Program.Budget,
                        a.Program.MaxBenefitPerCitizen,
                        a.Program.Status
                    },
                    EligibilityStatus = a.EligibilityChecks!.Any(e => e.Result == "Approved") ? "Approved" : "Pending"
                })
                .ToListAsync();

            return Ok(applications);
        }

        /// <summary>
        /// Get benefits allocated with program and citizen info
        /// </summary>
        [HttpGet("allocations")]
        public async Task<IActionResult> GetBenefitAllocations()
        {
            var allocations = await _context.Benefits
                .Include(b => b.WelfareApplication)
                    .ThenInclude(a => a!.Citizen)
                .Include(b => b.WelfareApplication)
                    .ThenInclude(a => a!.Program)
                .Include(b => b.Disbursements)
                .AsNoTracking()
                .Select(b => new
                {
                    b.BenefitID,
                    b.Amount,
                    b.Type,
                    b.Status,
                    b.Date,
                    Application = new
                    {
                        b.WelfareApplication!.ApplicationID,
                        b.WelfareApplication.Status
                    },
                    Citizen = new
                    {
                        b.WelfareApplication!.Citizen!.CitizenId,
                        b.WelfareApplication.Citizen.Name,
                        b.WelfareApplication.Citizen.ContactInfo
                    },
                    Program = new
                    {
                        b.WelfareApplication!.Program!.ProgramID,
                        b.WelfareApplication.Program.Title,
                        b.WelfareApplication.Program.Budget,
                        b.WelfareApplication.Program.MaxBenefitPerCitizen
                    },
                    TotalDisbursed = b.Disbursements!.Sum(d => d.Amount),
                    DisbursementCount = b.Disbursements!.Count,
                    RemainingAmount = b.Amount - b.Disbursements!.Sum(d => d.Amount)
                })
                .OrderByDescending(b => b.Date)
                .ToListAsync();

            return Ok(allocations);
        }

        /// <summary>
        /// Get open compliance issues
        /// </summary>
        [HttpGet("issues")]
        public async Task<IActionResult> GetComplianceIssues(int? officerID = null)
        {
            var issues = await _complianceService.GetComplianceIssuesAsync(officerID);

            var result = issues.Select(i => new
            {
                i.RecordID,
                i.EntityType,
                i.EntityId,
                i.ViolationType,
                i.Description,
                i.Status,
                i.CreatedDate,
                ApplicationID = i.ApplicationID,
                CitizenID = i.CitizenID,
                RaisedBy = i.RaisedByUser != null ? new { i.RaisedByUser.UserId, i.RaisedByUser.Username } : null
            }).ToList();

            return Ok(result);
        }

        /// <summary>
        /// Raise a compliance record for allocation
        /// </summary>
        [HttpPost("raise-compliance-allocation")]
        public async Task<IActionResult> RaiseComplianceForAllocation([FromQuery] int? benefitID, [FromQuery] int? applicationID, [FromBody] ComplianceRaiseRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Description))
                return BadRequest(new { Error = "Description is required" });

            // FIXED: Using JWT Claims instead of Session
            var userId = GetCurrentUserId();

            ComplainceRecord compliance;

            if (benefitID.HasValue)
            {
                var benefit = await _context.Benefits
                    .Include(b => b.WelfareApplication)
                    .FirstOrDefaultAsync(b => b.BenefitID == benefitID.Value);

                if (benefit == null)
                    return NotFound(new { Error = "Benefit not found" });

                compliance = new ComplainceRecord
                {
                    EntityType = "Benefit",
                    EntityId = benefitID.Value,
                    ApplicationID = benefit.ApplicationID,
                    CitizenID = benefit.WelfareApplication?.CitizenID,
                    ViolationType = request.ViolationType,
                    Description = request.Description,
                    Status = "Open",
                    RaisedByUserId = userId > 0 ? userId : null,
                    CreatedDate = DateTime.UtcNow
                };
            }
            else if (applicationID.HasValue)
            {
                var application = await _context.WelfareApplications.FindAsync(applicationID.Value);
                if (application == null)
                    return NotFound(new { Error = "Application not found" });

                compliance = new ComplainceRecord
                {
                    ApplicationID = applicationID.Value,
                    CitizenID = application.CitizenID,
                    EntityType = "Application",
                    EntityId = applicationID.Value,
                    ViolationType = request.ViolationType,
                    Description = request.Description,
                    Status = "Open",
                    RaisedByUserId = userId > 0 ? userId : null,
                    CreatedDate = DateTime.UtcNow
                };
            }
            else
            {
                return BadRequest(new { Error = "benefitID or applicationID is required" });
            }

            _context.ComplianceRecords.Add(compliance);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Compliance record raised successfully", RecordID = compliance.RecordID });
        }

        /// <summary>
        /// Raise a compliance record for disbursement
        /// </summary>
        [HttpPost("raise-compliance-disbursement")]
        public async Task<IActionResult> RaiseComplianceForDisbursement(int disbursementID, [FromBody] ComplianceRaiseRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Description))
                return BadRequest(new { Error = "Description is required" });

            var disbursement = await _context.Disbursements
                .Include(d => d.Benefit)
                .FirstOrDefaultAsync(d => d.DisbursementID == disbursementID);

            if (disbursement == null)
                return NotFound(new { Error = "Disbursement not found" });

            // FIXED: Using JWT Claims instead of Session
            var userId = GetCurrentUserId();

            var compliance = new ComplainceRecord
            {
                EntityType = "Disbursement",
                EntityId = disbursementID,
                ViolationType = request.ViolationType,
                Description = request.Description,
                Status = "Open",
                RaisedByUserId = userId > 0 ? userId : null,
                CreatedDate = DateTime.UtcNow
            };

            _context.ComplianceRecords.Add(compliance);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Compliance record raised successfully", RecordID = compliance.RecordID });
        }

        /// <summary>
        /// Resolve a compliance issue
        /// </summary>
        [HttpPut("resolve/{recordID}")]
        public async Task<IActionResult> ResolveCompliance(int recordID, [FromBody] ComplianceResolveRequest request)
        {
            var record = await _context.ComplianceRecords.FindAsync(recordID);
            if (record == null)
                return NotFound(new { Error = "Compliance record not found" });

            // FIXED: Using JWT Claims instead of Session
            var userId = GetCurrentUserId();

            record.Status = "Resolved";
            record.ResolvedDate = DateTime.UtcNow;
            record.ResolvedByUserId = userId > 0 ? userId : null;
            record.Notes = request.Notes;

            _context.ComplianceRecords.Update(record);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Compliance record resolved successfully" });
        }

        /// <summary>
        /// Trigger compliance checks
        /// </summary>
        [HttpPost("check-all")]
        public async Task<IActionResult> RunAllComplianceChecks()
        {
            await _complianceService.CheckDisbursementDelayComplianceAsync();
            return Ok(new { Message = "Compliance checks completed" });
        }

        /// <summary>
        /// Flag a welfare officer for non-compliance
        /// </summary>
        [HttpPost("flag-officer/{recordID}")]
        public async Task<IActionResult> FlagWelfareOfficer(int recordID, [FromBody] FlagOfficerRequest request)
        {
            var record = await _context.ComplianceRecords.FindAsync(recordID);
            if (record == null)
                return NotFound(new { Error = "Compliance record not found" });

            if (record.RaisedByUserId == null)
                return BadRequest(new { Error = "Cannot flag: No officer identified for this violation" });

            // FIXED: Using JWT Claims instead of Session
            var userId = GetCurrentUserId();

            // Create a new note indicating the officer has been flagged
            record.Notes = $"[FLAGGED] Officer flagged on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}: {request.Reason}";

            _context.ComplianceRecords.Update(record);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Welfare officer flagged successfully", RecordID = recordID });
        }

        /// <summary>
        /// Get compliance records for a specific officer
        /// </summary>
        [HttpGet("officer-violations/{officerID}")]
        public async Task<IActionResult> GetOfficerViolations(int officerID)
        {
            var violations = await _context.ComplianceRecords
                .Where(c => c.RaisedByUserId == officerID)
                .OrderByDescending(c => c.CreatedDate)
                .Select(c => new
                {
                    c.RecordID,
                    c.ViolationType,
                    c.Description,
                    c.Status,
                    c.CreatedDate,
                    c.ResolvedDate,
                    ResolvedBy = c.ResolvedByUser != null ? new { c.ResolvedByUser.UserId, c.ResolvedByUser.Username } : null,
                    c.Notes
                })
                .ToListAsync();

            return Ok(violations);
        }

        /// <summary>
        /// Get summary of compliance metrics
        /// </summary>
        [HttpGet("metrics")]
        public async Task<IActionResult> GetComplianceMetrics()
        {
            var totalIssues = await _context.ComplianceRecords.CountAsync();
            var openIssues = await _context.ComplianceRecords.CountAsync(c => c.Status == "Open");
            var resolvedIssues = await _context.ComplianceRecords.CountAsync(c => c.Status == "Resolved");

            var issuesByType = await _context.ComplianceRecords
                .Where(c => c.Status == "Open")
                .GroupBy(c => c.ViolationType)
                .Select(g => new { ViolationType = g.Key, Count = g.Count() })
                .ToListAsync();

            return Ok(new
            {
                Total = totalIssues,
                Open = openIssues,
                Resolved = resolvedIssues,
                IssuesByType = issuesByType
            });
        }

        /// <summary>
        /// Get compliance issues with advanced filters
        /// </summary>
        [HttpGet("issues/filtered")]
        public async Task<IActionResult> GetFilteredComplianceIssues(
            [FromQuery] string? status = null,
            [FromQuery] string? violationType = null,
            [FromQuery] int? citizenID = null,
            [FromQuery] int? benefitID = null)
        {
            try
            {
                var issues = await _complianceService.GetComplianceIssuesWithFiltersAsync(
                    status, violationType, citizenID, benefitID);

                var result = issues.Select(i => new
                {
                    i.RecordID,
                    i.EntityType,
                    i.EntityId,
                    i.ViolationType,
                    i.Description,
                    i.Status,
                    i.CreatedDate,
                    i.ResolvedDate,
                    ApplicationID = i.ApplicationID,
                    CitizenID = i.CitizenID,
                    RaisedBy = i.RaisedByUser != null ? new { i.RaisedByUser.UserId, i.RaisedByUser.Username } : null,
                    ResolvedBy = i.ResolvedByUser != null ? new { i.ResolvedByUser.UserId, i.ResolvedByUser.Username } : null,
                    i.Notes
                }).ToList();

                return Ok(new { success = true, count = result.Count, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get pending benefits requiring attention
        /// </summary>
        [HttpGet("pending-benefits")]
        public async Task<IActionResult> GetPendingBenefits([FromQuery] int? officerID = null)
        {
            try
            {
                var pendingBenefits = await _complianceService.GetPendingBenefitsAsync(officerID);

                var result = pendingBenefits.Select(b => new
                {
                    b.BenefitID,
                    b.Amount,
                    b.Type,
                    b.Status,
                    b.Date,
                    DaysElapsed = (DateTime.UtcNow - b.Date).Days,
                    Citizen = b.WelfareApplication?.Citizen?.Name ?? "Unknown",
                    Program = b.WelfareApplication?.Program?.Title ?? "Unknown",
                    MaxAllowedBenefit = b.WelfareApplication?.Program?.MaxBenefitPerCitizen ?? 0,
                    TotalDisbursed = b.Disbursements?.Sum(d => d.Amount) ?? 0,
                    RemainingToDisbuse = b.Amount - (b.Disbursements?.Sum(d => d.Amount) ?? 0),
                    DisbursementCount = b.Disbursements?.Count ?? 0
                }).ToList();

                return Ok(new { success = true, count = result.Count, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get pending disbursements requiring attention
        /// </summary>
        [HttpGet("pending-disbursements")]
        public async Task<IActionResult> GetPendingDisbursements()
        {
            try
            {
                var pendingDisbursements = await _complianceService.GetPendingDisbursementsAsync();

                var result = pendingDisbursements.Select(d => new
                {
                    d.DisbursementID,
                    d.BenefitID,
                    d.Amount,
                    d.Status,
                    d.Date,
                    DaysElapsed = (DateTime.UtcNow - d.Date).Days,
                    CitizenID = d.CitizenID,
                    OfficerID = d.OfficerID,
                    BenefitAmount = d.Benefit?.Amount ?? 0,
                    DisbursedPercent = d.Benefit != null ? ((d.Amount / d.Benefit.Amount) * 100) : 0
                }).ToList();

                return Ok(new { success = true, count = result.Count, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get compliance history for a citizen or benefit
        /// </summary>
        [HttpGet("history")]
        public async Task<IActionResult> GetComplianceHistory(
            [FromQuery] int? citizenID = null,
            [FromQuery] int? benefitID = null)
        {
            try
            {
                var history = await _complianceService.GetComplianceHistoryAsync(citizenID, benefitID);

                var result = history.Select(h => new
                {
                    h.RecordID,
                    h.ViolationType,
                    h.Description,
                    h.Status,
                    h.CreatedDate,
                    h.ResolvedDate,
                    DaysOpen = h.ResolvedDate.HasValue ? (h.ResolvedDate.Value - h.CreatedDate).Days : (DateTime.UtcNow - h.CreatedDate).Days,
                    RaisedBy = h.RaisedByUser?.Username ?? "System",
                    ResolvedBy = h.ResolvedByUser?.Username,
                    h.Notes
                }).ToList();

                return Ok(new { success = true, count = result.Count, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get all applications with detailed benefit and disbursement information for compliance officer dashboard
        /// </summary>
        [HttpGet("dashboard/applications-list")]
        public async Task<IActionResult> GetApplicationsForDashboard()
        {
            try
            {
                var applications = await _context.WelfareApplications
                    .Include(a => a.Citizen)
                    .Include(a => a.Program)
                    .Include(a => a.Benefits)
                        .ThenInclude(b => b.Disbursements)
                    .AsNoTracking()
                    .OrderByDescending(a => a.SubmittedDate)
                    .ToListAsync();

                var now = DateTime.UtcNow;

                var result = applications.Select(a => new
                {
                    ApplicationID = a.ApplicationID,
                    CitizenName = a.Citizen!.Name,
                    CitizenID = a.Citizen.CitizenId,
                    ProgramTitle = a.Program!.Title,
                    ProgramID = a.Program.ProgramID,
                    ApplicationStatus = a.Status,
                    SubmittedDate = a.SubmittedDate,
                    MaxBenefit = a.Program.MaxBenefitPerCitizen,
                    TotalBenefitAllocated = a.Benefits!.Sum(b => b.Amount),
                    TotalDisbursed = a.Benefits!.Sum(b => b.Disbursements!.Sum(d => d.Amount)),
                    RemainingToDisborse = a.Benefits!.Sum(b => b.Amount) - a.Benefits!.Sum(b => b.Disbursements!.Sum(d => d.Amount)),
                    BenefitCount = a.Benefits!.Count,
                    DisbursementCount = a.Benefits!.Sum(b => b.Disbursements!.Count),
                    Benefits = a.Benefits!.Select(b => new
                    {
                        BenefitID = b.BenefitID,
                        BenefitType = b.Type,
                        BenefitAmount = b.Amount,
                        BenefitStatus = b.Status,
                        BenefitDate = b.Date,
                        DaysAllocated = (now - b.Date).Days,
                        DisbursementCount = b.Disbursements!.Count,
                        TotalBenefitDisbursed = b.Disbursements!.Sum(d => d.Amount),
                        RemainingBenefit = b.Amount - b.Disbursements!.Sum(d => d.Amount),
                        Disbursements = b.Disbursements!.Select(d => new
                        {
                            DisbursementID = d.DisbursementID,
                            Amount = d.Amount,
                            Date = d.Date,
                            Status = d.Status
                        }).ToList()
                    }).ToList(),
                    IsPendingAllocation = a.Benefits!.Count == 0 && a.Status == "Approved" && (now - a.SubmittedDate.ToDateTime(TimeOnly.MinValue)).Days >= 2,
                    HasNoDisbursement = a.Benefits!.Any(b => b.Disbursements!.Count == 0) && a.Benefits!.Count > 0 && a.Benefits!.Any(b => (now - b.Date).Days >= 2)
                    ,
                    // Indicate whether there is any non-final compliance record for this application
                    // (Open, Under Investigation, and Dismissed are all treated as active/red flag states)
                    IsFlagged = _context.ComplianceRecords.Any(c => c.ApplicationID == a.ApplicationID && c.Status != "Resolved")
                }).ToList();

                return Ok(new { success = true, count = result.Count, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }

    public class ComplianceRaiseRequest
    {
        public string ViolationType { get; set; }
        public string Description { get; set; }
    }

    public class ComplianceResolveRequest
    {
        public string? Notes { get; set; }
    }

    public class FlagOfficerRequest
    {
        public string Reason { get; set; }
    }
}