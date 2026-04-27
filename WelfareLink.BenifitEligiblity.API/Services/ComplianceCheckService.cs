using WelfareLink.BenifitEligiblity.API.Data;
using WelfareLink.BenifitEligiblity.API.Interfaces;
using WelfareLink.BenifitEligiblity.API.Models;
using Microsoft.EntityFrameworkCore;

namespace WelfareLink.BenifitEligiblity.API.Services
{
    /// <summary>
    /// Service to check compliance rules and flag violations
    /// </summary>
    public class ComplianceCheckService : IComplianceCheckService
    {
        private readonly WelfareLinkDbContext _context;

        public ComplianceCheckService(WelfareLinkDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Check if benefit exceeds max allowed for the program and citizen
        /// </summary>
        public async Task CheckMaxBenefitComplianceAsync(int benefitID)
        {
            var benefit = await _context.Benefits
                .Include(b => b.WelfareApplication)
                    .ThenInclude(a => a.Program)
                .FirstOrDefaultAsync(b => b.BenefitID == benefitID);

            if (benefit == null) return;

            var program = benefit.WelfareApplication?.Program;
            var citizenID = benefit.WelfareApplication?.CitizenID;

            if (program?.MaxBenefitPerCitizen > 0 && citizenID.HasValue)
            {
                // Get total benefits for this citizen in this program
                var totalBenefit = await _context.Benefits
                    .Where(b => b.WelfareApplication!.ProgramID == program.ProgramID &&
                                b.WelfareApplication!.CitizenID == citizenID &&
                                b.Status != "Failed" &&
                                b.Status != "Cancelled")
                    .SumAsync(b => b.Amount);

                if (totalBenefit > (double)program.MaxBenefitPerCitizen)
                    {
                        // Create compliance record
                        var existingRecord = await _context.ComplianceRecords
                            .FirstOrDefaultAsync(c => c.EntityType == "Benefit" &&
                                                       c.EntityId == benefitID && 
                                                       c.ViolationType == "MaxBenefitExceeded" &&
                                                       c.Status == "Open");

                        if (existingRecord == null)
                        {
                            var compliance = new ComplainceRecord
                            {
                                EntityType = "Benefit",
                                EntityId = benefitID,
                                ApplicationID = benefit.ApplicationID,
                                CitizenID = citizenID,
                                ViolationType = "MaxBenefitExceeded",
                                Description = $"Citizen {citizenID} total benefit (Rs. {totalBenefit}) exceeds max allowed (Rs. {program.MaxBenefitPerCitizen}) in program {program.Title}",
                                Status = "Open"
                            };

                            _context.ComplianceRecords.Add(compliance);
                            await _context.SaveChangesAsync();
                        }
                    }
            }
        }

        /// <summary>
        /// Check if disbursements haven't completed within 2 days and flag them
        /// </summary>
        public async Task CheckDisbursementDelayComplianceAsync()
        {
            var twoDaysAgo = DateTime.UtcNow.AddDays(-2);

            // Find benefits created 2 days ago but not fully disbursed
            var delayedBenefits = await _context.Benefits
                .Where(b => b.Date <= twoDaysAgo &&
                            (b.Status == "Pending" || b.Status == "InProgress"))
                .Include(b => b.Disbursements)
                .Include(b => b.WelfareApplication)
                .ToListAsync();

            foreach (var benefit in delayedBenefits)
            {
                var totalDisbursed = benefit.Disbursements?.Sum(d => d.Amount) ?? 0;
                var isFullyDisbursed = totalDisbursed >= benefit.Amount;

                if (!isFullyDisbursed)
                    {
                        // Check if compliance record already exists
                        var existingRecord = await _context.ComplianceRecords
                            .FirstOrDefaultAsync(c => c.EntityType == "Benefit" &&
                                                       c.EntityId == benefit.BenefitID &&
                                                       c.ViolationType == "DisbursementDelayed" &&
                                                       c.Status == "Open");

                        if (existingRecord == null)
                        {
                            var compliance = new ComplainceRecord
                            {
                                EntityType = "Benefit",
                                EntityId = benefit.BenefitID,
                                ApplicationID = benefit.ApplicationID,
                                CitizenID = benefit.WelfareApplication?.CitizenID,
                                ViolationType = "DisbursementDelayed",
                                Description = $"Benefit #{benefit.BenefitID} (Rs. {benefit.Amount}) created on {benefit.Date:yyyy-MM-dd} not completed within 2 days. Disbursed: Rs. {totalDisbursed}",
                                Status = "Open"
                            };

                            _context.ComplianceRecords.Add(compliance);
                        }
                    }
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Get all open compliance issues with optional filtering
        /// </summary>
        public async Task<List<ComplainceRecord>> GetComplianceIssuesAsync(int? officerID = null)
        {
            var query = _context.ComplianceRecords
                .Where(c => c.Status == "Open")
                .AsQueryable();

            if (officerID.HasValue)
            {
                query = query.Where(c => c.RaisedByUserId == officerID || c.RaisedByUserId == null);
            }

            return await query
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Get compliance issues with advanced filtering
        /// </summary>
        public async Task<List<ComplainceRecord>> GetComplianceIssuesWithFiltersAsync(
            string? status = null, 
            string? violationType = null,
            int? citizenID = null,
            int? benefitID = null)
        {
            var query = _context.ComplianceRecords.AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(c => c.Status == status);

            if (!string.IsNullOrEmpty(violationType))
                query = query.Where(c => c.ViolationType == violationType);

            if (citizenID.HasValue)
                query = query.Where(c => c.CitizenID == citizenID);

            if (benefitID.HasValue)
                query = query.Where(c => c.EntityType == "Benefit" && c.EntityId == benefitID);

            return await query
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Get pending benefits for a specific officer
        /// </summary>
        public async Task<List<Benefit>> GetPendingBenefitsAsync(int? officerID = null)
        {
            var query = _context.Benefits
                .Where(b => b.Status == "Pending" || b.Status == "InProgress")
                .Include(b => b.WelfareApplication)
                .Include(b => b.Disbursements)
                .AsQueryable();

            return await query
                .OrderBy(b => b.Date)
                .ToListAsync();
        }

        /// <summary>
        /// Get pending disbursements
        /// </summary>
        public async Task<List<Disbursement>> GetPendingDisbursementsAsync()
        {
            return await _context.Disbursements
                .Where(d => d.Status == "Pending" || d.Status == "InProgress")
                .Include(d => d.Benefit)
                .OrderBy(d => d.Date)
                .ToListAsync();
        }

        /// <summary>
        /// Resolve/mark a compliance issue as compliant
        /// </summary>
        public async Task MarkComplianceAsResolvedAsync(int recordID, int? resolvedByUserId, string notes = "")
        {
            var record = await _context.ComplianceRecords
                .FirstOrDefaultAsync(c => c.RecordID == recordID);

            if (record != null)
            {
                record.Status = "Resolved";
                record.ResolvedDate = DateTime.UtcNow;
                record.ResolvedByUserId = resolvedByUserId;
                record.Notes = notes;

                _context.ComplianceRecords.Update(record);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Flag an officer for follow-up
        /// </summary>
        public async Task FlagOfficerAsync(int officerID, int? complianceRecordID, string reason, int? flaggedByUserId = null)
        {
            var flag = new ComplainceRecord
            {
                RaisedByUserId = flaggedByUserId,
                EntityType = "Officer",
                EntityId = officerID,
                ViolationType = "OfficerFlagged",
                Description = reason,
                Status = "Open",
                CreatedDate = DateTime.UtcNow
            };

            _context.ComplianceRecords.Add(flag);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Get audit trail for compliance tracking
        /// </summary>
        public async Task<List<ComplainceRecord>> GetComplianceHistoryAsync(int? citizenID = null, int? benefitID = null)
        {
            var query = _context.ComplianceRecords.AsQueryable();

            if (citizenID.HasValue)
                query = query.Where(c => c.CitizenID == citizenID);

            if (benefitID.HasValue)
                query = query.Where(c => c.EntityType == "Benefit" && c.EntityId == benefitID);

            return await query
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
        }
    }
}
