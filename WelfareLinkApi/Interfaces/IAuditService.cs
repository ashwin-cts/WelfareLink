using WelfareLinkApi.Models;

namespace WelfareLinkApi.Interfaces;

public interface IAuditService
{
    Task<IEnumerable<Audit>> GetAllAuditsAsync();
    Task<Audit?> GetAuditByIdAsync(int id);
    Task<IEnumerable<Audit>> GetAuditsByProgramAsync(int programId);
    Task<Audit> CreateAuditAsync(Audit audit);
    Task<Audit> UpdateAuditStatusAsync(int id, string status);
    Task<IEnumerable<ProgramAuditSummary>> GetGovernmentAuditorDashboardAsync();
}

public class ProgramAuditSummary
{
    public int ProgramID { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Budget { get; set; }
    public string Status { get; set; } = string.Empty;
    public int TotalBeneficiaries { get; set; }
    public double TotalBenefitAmount { get; set; }
    public double TotalDisbursed { get; set; }
    public double RemainingBudget { get; set; }
    public int TotalResources { get; set; }
    public int OpenAudits { get; set; }
}
