using Microsoft.EntityFrameworkCore;
using WelfareLinkApi.Data;
using WelfareLinkApi.Interfaces;
using WelfareLinkApi.Models;

namespace WelfareLinkApi.Services;

public class AuditService : IAuditService
{
    private readonly IAuditRepository _repo;
    private readonly WelfareLinkDbContext _context;

    public AuditService(IAuditRepository repo, WelfareLinkDbContext context)
    {
        _repo = repo;
        _context = context;
    }

    public async Task<IEnumerable<Audit>> GetAllAuditsAsync()
        => await _repo.GetAllAsync();

    public async Task<Audit?> GetAuditByIdAsync(int id)
        => await _repo.GetByIdAsync(id);

    public async Task<IEnumerable<Audit>> GetAuditsByProgramAsync(int programId)
        => await _repo.GetByProgramIdAsync(programId);

    public async Task<Audit> CreateAuditAsync(Audit audit)
    {
        audit.AuditDate = DateTime.UtcNow;
        audit.Status = "Open";
        return await _repo.AddAsync(audit);
    }

    public async Task<Audit> UpdateAuditStatusAsync(int id, string status)
    {
        var audit = await _repo.GetByIdAsync(id)
            ?? throw new InvalidOperationException($"Audit #{id} not found.");
        audit.Status = status;
        if (status == "Resolved") audit.ResolvedDate = DateTime.UtcNow;
        await _repo.UpdateAsync(audit);
        return audit;
    }

    public async Task<IEnumerable<ProgramAuditSummary>> GetGovernmentAuditorDashboardAsync()
    {
        var programs = await _context.Programs.ToListAsync();
        var resources = await _context.Resources.ToListAsync();
        var audits = await _context.Audits.ToListAsync();

        var applications = await _context.WelfareApplications
            .Where(a => a.Status == "Approved" || a.Status == "Fully Disbursed")
            .ToListAsync();

        var benefits = await _context.Benefits.ToListAsync();
        var disbursements = await _context.Disbursements.ToListAsync();

        return programs.Select(p =>
        {
            var appIds = applications.Where(a => a.ProgramID == p.ProgramID).Select(a => a.ApplicationID).ToHashSet();
            var progBenefits = benefits.Where(b => appIds.Contains(b.ApplicationID)).ToList();
            var benefitIds = progBenefits.Select(b => b.BenefitID).ToHashSet();
            var progDisb = disbursements.Where(d => benefitIds.Contains(d.BenefitID)).ToList();

            var totalDisbursed = progDisb.Sum(d => d.Amount);
            var openAudits = audits.Count(a => a.ProgramID == p.ProgramID && a.Status == "Open");

            return new ProgramAuditSummary
            {
                ProgramID = p.ProgramID,
                Title = p.Title,
                Description = p.Description,
                Budget = p.Budget,
                Status = p.Status ?? "-",
                TotalBeneficiaries = appIds.Count,
                TotalBenefitAmount = progBenefits.Sum(b => b.Amount),
                TotalDisbursed = totalDisbursed,
                RemainingBudget = (double)p.Budget - totalDisbursed,
                TotalResources = (int)resources.Where(r => r.ProgramID == p.ProgramID).Sum(r => r.Quantity),
                OpenAudits = openAudits
            };
        }).ToList();
    }
}
