using WelfareLink.AnalyticsReport.API.Data;
using WelfareLink.AnalyticsReport.API.Interfaces;
using WelfareLink.AnalyticsReport.API.Models;
using Microsoft.EntityFrameworkCore;

namespace WelfareLink.AnalyticsReport.API.Services;

public class ComplainceRecordService : IComplainceRecordService
{
    private readonly IComplainceRecordRepository _repo;
    private readonly WelfareLinkDbContext _context;

    public ComplainceRecordService(IComplainceRecordRepository repo, WelfareLinkDbContext context)
    {
        _repo = repo;
        _context = context;
    }

    public async Task<IEnumerable<ComplainceRecord>> GetAllRecordsAsync()
        => await _repo.GetAllAsync();

    public async Task<ComplainceRecord?> GetRecordByIdAsync(int id)
        => await _repo.GetByIdAsync(id);

    public async Task<IEnumerable<ComplainceRecord>> GetOpenRecordsAsync()
        => await _repo.GetByStatusAsync("Open");

    public async Task<ComplainceRecord> CreateRecordAsync(ComplainceRecord record)
    {
        record.CreatedDate = DateTime.UtcNow;
        record.Status = "Open";

        // Auto-populate ApplicationID and CitizenID based on EntityType and EntityId
        await PopulateApplicationAndCitizenIdAsync(record);

        return await _repo.AddAsync(record);
    }

    public async Task<ComplainceRecord> UpdateStatusAsync(int id, string status, int? resolvedByUserId, string? notes)
    {
        var record = await _repo.GetByIdAsync(id)
            ?? throw new InvalidOperationException($"Compliance record #{id} not found.");

        record.Status = status;
        if (notes != null) record.Notes = notes;
        if (status is "Resolved" or "Dismissed")
        {
            record.ResolvedDate = DateTime.UtcNow;
            record.ResolvedByUserId = resolvedByUserId;
        }
        await _repo.UpdateAsync(record);
        return record;
    }

    private async Task PopulateApplicationAndCitizenIdAsync(ComplainceRecord record)
    {
        if (string.IsNullOrEmpty(record.EntityType) || record.EntityId <= 0)
            return;

        try
        {
            switch (record.EntityType.ToLower())
            {
                case "benefit":
                    var benefit = await _context.Benefits.FindAsync(record.EntityId);
                    if (benefit != null)
                    {
                        record.ApplicationID = benefit.ApplicationID;
                        var benefitApp = await _context.WelfareApplications.FindAsync(benefit.ApplicationID);
                        if (benefitApp != null)
                            record.CitizenID = benefitApp.CitizenID;
                    }
                    break;

                case "disbursement":
                    var disbursement = await _context.Disbursements.FindAsync(record.EntityId);
                    if (disbursement != null)
                    {
                        var benefitForDisbursement = await _context.Benefits.FindAsync(disbursement.BenefitID);
                        if (benefitForDisbursement != null)
                        {
                            record.ApplicationID = benefitForDisbursement.ApplicationID;
                            var disbursementApp = await _context.WelfareApplications.FindAsync(benefitForDisbursement.ApplicationID);
                            if (disbursementApp != null)
                                record.CitizenID = disbursementApp.CitizenID;
                        }
                    }
                    break;

                case "eligibilitycheck":
                    var eligibilityCheck = await _context.EligibilityChecks.FindAsync(record.EntityId);
                    if (eligibilityCheck != null)
                    {
                        var checkApp = await _context.WelfareApplications.FindAsync(eligibilityCheck.ApplicationID);
                        if (checkApp != null)
                        {
                            record.ApplicationID = checkApp.ApplicationID;
                            record.CitizenID = checkApp.CitizenID;
                        }
                    }
                    break;

                case "application":
                    var app = await _context.WelfareApplications.FindAsync(record.EntityId);
                    if (app != null)
                    {
                        record.ApplicationID = app.ApplicationID;
                        record.CitizenID = app.CitizenID;
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error populating ApplicationID and CitizenID: {ex.Message}");
            // Don't throw - allow record creation even if auto-population fails
        }
    }
}
