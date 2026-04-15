using WelfareLinkApi.Models;
using WelfareLinkApi.Services;

namespace WelfareLinkApi.Interfaces
{
    public interface IAuditMonitoringService
    {
        // Report generation
        Task<ProgramAuditReport> GetProgramAuditReportAsync(int programID);
        Task<List<ResourceAllocationSummary>> GetResourceAllocationSummaryAsync(int programID);
        Task<List<BudgetTrackingReport>> GetComprehensiveBudgetTrackingAsync();
        Task<MoneyFlowAnalysis> GetMoneyFlowAnalysisAsync(int programID);

        // Resource management
        Task<List<Resource>> GetPendingResourcesAsync(int? programID = null);
        Task ApproveResourceAsync(int resourceID, int approvedByUserId, string notes = "");
        Task FlagResourceAsInsufficientAsync(int resourceID, int auditedByUserId, string reason);

        // Audit findings
        Task<List<Audit>> GetOpenAuditFindingsAsync(int? programID = null);
        Task CloseAuditFindingAsync(int auditID, string resolutionNotes);

        // Audit trail
        Task<List<AuditLog>> GetProgramAuditTrailAsync(int programID, DateTime? from = null, DateTime? to = null);
    }
}
