using WelfareLink.ProgramManagement.API.Models;

namespace WelfareLink.ProgramManagement.API.Interfaces
{
    public interface IComplianceCheckService
    {
        // Core compliance checks
        Task CheckMaxBenefitComplianceAsync(int benefitID);
        Task CheckDisbursementDelayComplianceAsync();

        // Retrieve compliance issues
        Task<List<ComplainceRecord>> GetComplianceIssuesAsync(int? officerID = null);
        Task<List<ComplainceRecord>> GetComplianceIssuesWithFiltersAsync(
            string? status = null,
            string? violationType = null,
            int? citizenID = null,
            int? benefitID = null);

        // Pending items filtering
        Task<List<Benefit>> GetPendingBenefitsAsync(int? officerID = null);
        Task<List<Disbursement>> GetPendingDisbursementsAsync();

        // Resolution and flagging
        Task MarkComplianceAsResolvedAsync(int recordID, int? resolvedByUserId, string notes = "");
        Task FlagOfficerAsync(int officerID, int? complianceRecordID, string reason, int? flaggedByUserId = null);

        // Audit trail
        Task<List<ComplainceRecord>> GetComplianceHistoryAsync(int? citizenID = null, int? benefitID = null);
    }
}
