using WelfareLinkApi.Data;
using WelfareLinkApi.Interfaces;
using WelfareLinkApi.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace WelfareLinkApi.Services
{
    /// <summary>
    /// Service for government auditors to monitor overall money flow, resource allocation, and program performance
    /// </summary>
    public class AuditMonitoringService : IAuditMonitoringService
    {
        private readonly WelfareLinkDbContext _context;
        private readonly IAuditLogServiceEnhanced _auditLogService;

        public AuditMonitoringService(WelfareLinkDbContext context, IAuditLogServiceEnhanced auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }

        /// <summary>
        /// Get comprehensive audit data for a program
        /// </summary>
        public async Task<ProgramAuditReport> GetProgramAuditReportAsync(int programID)
        {
            var program = await _context.Programs
                .Include(p => p.Resources)
                .Include(p => p.WelfareApplications)
                .FirstOrDefaultAsync(p => p.ProgramID == programID);

            if (program == null)
                throw new InvalidOperationException($"Program {programID} not found");

            // Calculate budget metrics
            var totalBenefitsAllocated = await _context.Benefits
                .Where(b => b.WelfareApplication!.ProgramID == programID && 
                           b.Status != "Failed" && b.Status != "Cancelled")
                .SumAsync(b => (decimal?)b.Amount) ?? 0;

            var totalDisbursed = await _context.Disbursements
                .Where(d => d.Benefit!.WelfareApplication!.ProgramID == programID &&
                           d.Status != "Failed" && d.Status != "Cancelled")
                .SumAsync(d => (decimal?)d.Amount) ?? 0;

            var pendingBenefits = await _context.Benefits
                .Where(b => b.WelfareApplication!.ProgramID == programID &&
                           (b.Status == "Pending" || b.Status == "InProgress"))
                .CountAsync();

            var pendingDisbursements = await _context.Disbursements
                .Where(d => d.Benefit!.WelfareApplication!.ProgramID == programID &&
                           (d.Status == "Pending" || d.Status == "InProgress"))
                .CountAsync();

            var report = new ProgramAuditReport
            {
                ProgramID = programID,
                ProgramTitle = program.Title,
                TotalBudget = program.Budget,
                AllocatedBenefits = totalBenefitsAllocated,
                DisbursedAmount = totalDisbursed,
                PendingBenefitsCount = pendingBenefits,
                PendingDisbursementsCount = pendingDisbursements,
                RemainingBudget = program.Budget - totalBenefitsAllocated,
                BudgetUtilizationPercentage = program.Budget > 0 
                    ? ((double)(totalBenefitsAllocated / program.Budget) * 100) 
                    : 0,
                ResourcesTotalValue = program.Resources?.Sum(r => r.Quantity) ?? 0,
                TotalApplications = program.WelfareApplications?.Count ?? 0,
                ApprovedApplications = program.WelfareApplications?.Count(a => a.Status == "Approved") ?? 0,
                ReportDate = DateTime.UtcNow
            };

            return report;
        }

        /// <summary>
        /// Get resource allocation summary for auditing
        /// </summary>
        public async Task<List<ResourceAllocationSummary>> GetResourceAllocationSummaryAsync(int programID)
        {
            var resources = await _context.Resources
                .Where(r => r.ProgramID == programID)
                .ToListAsync();

            var summary = new List<ResourceAllocationSummary>();

            foreach (var resource in resources)
            {
                summary.Add(new ResourceAllocationSummary
                {
                    ResourceID = resource.ResourceID,
                    ResourceType = resource.Type,
                    AllocatedQuantity = resource.Quantity,
                    Status = resource.Status,
                    LastUpdated = DateTime.UtcNow
                });
            }

            return summary;
        }

        /// <summary>
        /// Flag a resource as insufficient and create audit finding
        /// </summary>
        public async Task FlagResourceAsInsufficientAsync(int resourceID, int auditedByUserId, string reason)
        {
            var resource = await _context.Resources
                .Include(r => r.Program)
                .FirstOrDefaultAsync(r => r.ResourceID == resourceID);

            if (resource == null)
                throw new InvalidOperationException($"Resource {resourceID} not found");

            var audit = new Audit
            {
                ProgramID = resource.ProgramID,
                AuditedByUserId = auditedByUserId,
                AuditDate = DateTime.UtcNow,
                FindingType = "InsufficientResource",
                Description = $"Resource '{resource.Type}' flagged as insufficient. Current allocation: {resource.Quantity}. Reason: {reason}",
                Status = "Open"
            };

            _context.Audits.Add(audit);
            await _context.SaveChangesAsync();

            // Log this action
            await _auditLogService.LogUserActionAsync(
                auditedByUserId,
                "FLAG",
                "Resource",
                resourceID,
                $"Resource flagged as insufficient: {resource.Type}",
                null,
                $"Status: Flagged, Quantity: {resource.Quantity}"
            );
        }

        /// <summary>
        /// Get all open audit findings for a program
        /// </summary>
        public async Task<List<Audit>> GetOpenAuditFindingsAsync(int? programID = null)
        {
            var query = _context.Audits
                .Where(a => a.Status == "Open")
                .Include(a => a.WelfareProgram)
                .Include(a => a.AuditedByUser)
                .AsQueryable();

            if (programID.HasValue)
                query = query.Where(a => a.ProgramID == programID);

            return await query
                .OrderByDescending(a => a.AuditDate)
                .ToListAsync();
        }

        /// <summary>
        /// Close an audit finding
        /// </summary>
        public async Task CloseAuditFindingAsync(int auditID, string resolutionNotes)
        {
            var audit = await _context.Audits.FirstOrDefaultAsync(a => a.AuditID == auditID);

            if (audit == null)
                throw new InvalidOperationException($"Audit {auditID} not found");

            audit.Status = "Resolved";
            audit.ResolvedDate = DateTime.UtcNow;

            _context.Audits.Update(audit);
            await _context.SaveChangesAsync();

            // Log the resolution
            await _auditLogService.LogUserActionAsync(
                audit.AuditedByUserId,
                "RESOLVE",
                "Audit",
                auditID,
                $"Audit finding resolved: {audit.FindingType}",
                "Open",
                "Resolved",
                null,
                resolutionNotes
            );
        }

        /// <summary>
        /// Get comprehensive budget tracking across all programs
        /// </summary>
        public async Task<List<BudgetTrackingReport>> GetComprehensiveBudgetTrackingAsync()
        {
            var programs = await _context.Programs.ToListAsync();
            var reports = new List<BudgetTrackingReport>();

            foreach (var program in programs)
            {
                var totalAllocated = await _context.Benefits
                    .Where(b => b.WelfareApplication!.ProgramID == program.ProgramID &&
                               b.Status != "Failed" && b.Status != "Cancelled")
                    .SumAsync(b => (decimal?)b.Amount) ?? 0;

                var totalDisbursed = await _context.Disbursements
                    .Where(d => d.Benefit!.WelfareApplication!.ProgramID == program.ProgramID &&
                               d.Status != "Failed" && d.Status != "Cancelled")
                    .SumAsync(d => (decimal?)d.Amount) ?? 0;

                reports.Add(new BudgetTrackingReport
                {
                    ProgramID = program.ProgramID,
                    ProgramTitle = program.Title,
                    TotalBudget = program.Budget,
                    AllocatedBenefits = totalAllocated,
                    DisbursedAmount = totalDisbursed,
                    RemainingBudget = program.Budget - totalAllocated,
                    BudgetUtilizationPercentage = program.Budget > 0 
                        ? ((double)(totalAllocated / program.Budget) * 100) 
                        : 0,
                    Status = program.Status ?? "Active",
                    LastUpdated = DateTime.UtcNow
                });
            }

            return reports.OrderByDescending(r => r.BudgetUtilizationPercentage).ToList();
        }

        /// <summary>
        /// Get pending resources that need approval
        /// </summary>
        public async Task<List<Resource>> GetPendingResourcesAsync(int? programID = null)
        {
            var query = _context.Resources
                .Where(r => r.Status == "Pending")
                .Include(r => r.Program)
                .AsQueryable();

            if (programID.HasValue)
                query = query.Where(r => r.ProgramID == programID);

            return await query.ToListAsync();
        }

        /// <summary>
        /// Approve a pending resource
        /// </summary>
        public async Task ApproveResourceAsync(int resourceID, int approvedByUserId, string notes = "")
        {
            var resource = await _context.Resources
                .Include(r => r.Program)
                .FirstOrDefaultAsync(r => r.ResourceID == resourceID);

            if (resource == null)
                throw new InvalidOperationException($"Resource {resourceID} not found");

            resource.Status = "Approved";
            _context.Resources.Update(resource);
            await _context.SaveChangesAsync();

            // Log the approval
            await _auditLogService.LogUserActionAsync(
                approvedByUserId,
                "APPROVE",
                "Resource",
                resourceID,
                $"Resource approved: {resource.Type}",
                "Pending",
                "Approved",
                null,
                notes
            );
        }

        /// <summary>
        /// Get audit trail for a specific program
        /// </summary>
        public async Task<List<AuditLog>> GetProgramAuditTrailAsync(int programID, DateTime? from = null, DateTime? to = null)
        {
            var query = _context.AuditLogs
                .Where(l => l.EntityType == "Program" && l.EntityId == programID)
                .AsQueryable();

            if (from.HasValue)
                query = query.Where(l => l.Timestamp >= from);

            if (to.HasValue)
                query = query.Where(l => l.Timestamp <= to);

            return await query
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
        }

        /// <summary>
        /// Get money flow analysis for a program
        /// </summary>
        public async Task<MoneyFlowAnalysis> GetMoneyFlowAnalysisAsync(int programID)
        {
            var program = await _context.Programs
                .FirstOrDefaultAsync(p => p.ProgramID == programID);

            if (program == null)
                throw new InvalidOperationException($"Program {programID} not found");

            var allocatedBenefits = await _context.Benefits
                .Where(b => b.WelfareApplication!.ProgramID == programID &&
                           b.Status != "Failed" && b.Status != "Cancelled")
                .ToListAsync();

            var disbursements = await _context.Disbursements
                .Where(d => d.Benefit!.WelfareApplication!.ProgramID == programID &&
                           d.Status != "Failed" && d.Status != "Cancelled")
                .ToListAsync();

            var totalAllocated = allocatedBenefits.Sum(b => (decimal)b.Amount);
            var totalDisbursed = disbursements.Sum(d => (decimal)d.Amount);

            return new MoneyFlowAnalysis
            {
                ProgramID = programID,
                ProgramTitle = program.Title,
                ProgramBudget = program.Budget,
                TotalAllocated = totalAllocated,
                TotalDisbursed = totalDisbursed,
                PendingDisbursement = totalAllocated - totalDisbursed,
                NumberOfBeneficiaries = allocatedBenefits.GroupBy(b => b.WelfareApplication!.CitizenID).Count(),
                AverageBenefitAmount = allocatedBenefits.Count > 0 
                    ? (decimal)allocatedBenefits.Average(b => b.Amount) 
                    : 0,
                BudgetRemaining = program.Budget - totalAllocated,
                AllocationDate = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// DTO for program audit report
    /// </summary>
    public class ProgramAuditReport
    {
        public int ProgramID { get; set; }
        public string ProgramTitle { get; set; } = string.Empty;
        public decimal TotalBudget { get; set; }
        public decimal AllocatedBenefits { get; set; }
        public decimal DisbursedAmount { get; set; }
        public int PendingBenefitsCount { get; set; }
        public int PendingDisbursementsCount { get; set; }
        public decimal RemainingBudget { get; set; }
        public double BudgetUtilizationPercentage { get; set; }
        public decimal ResourcesTotalValue { get; set; }
        public int TotalApplications { get; set; }
        public int ApprovedApplications { get; set; }
        public DateTime ReportDate { get; set; }
    }

    /// <summary>
    /// DTO for resource allocation summary
    /// </summary>
    public class ResourceAllocationSummary
    {
        public int ResourceID { get; set; }
        public string ResourceType { get; set; } = string.Empty;
        public decimal AllocatedQuantity { get; set; }
        public string? Status { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// DTO for budget tracking report
    /// </summary>
    public class BudgetTrackingReport
    {
        public int ProgramID { get; set; }
        public string ProgramTitle { get; set; } = string.Empty;
        public decimal TotalBudget { get; set; }
        public decimal AllocatedBenefits { get; set; }
        public decimal DisbursedAmount { get; set; }
        public decimal RemainingBudget { get; set; }
        public double BudgetUtilizationPercentage { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// DTO for money flow analysis
    /// </summary>
    public class MoneyFlowAnalysis
    {
        public int ProgramID { get; set; }
        public string ProgramTitle { get; set; } = string.Empty;
        public decimal ProgramBudget { get; set; }
        public decimal TotalAllocated { get; set; }
        public decimal TotalDisbursed { get; set; }
        public decimal PendingDisbursement { get; set; }
        public int NumberOfBeneficiaries { get; set; }
        public decimal AverageBenefitAmount { get; set; }
        public decimal BudgetRemaining { get; set; }
        public DateTime AllocationDate { get; set; }
    }
}
