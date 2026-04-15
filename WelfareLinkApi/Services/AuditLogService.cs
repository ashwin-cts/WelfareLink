using WelfareLinkApi.Data;
using WelfareLinkApi.Interfaces;
using WelfareLinkApi.Models;
using Microsoft.EntityFrameworkCore;

namespace WelfareLinkApi.Services
{
    public class AuditLogService : IAuditLogServiceEnhanced
    {
        private readonly IAuditLogRepository _repo;
        private readonly WelfareLinkDbContext _context;

        public AuditLogService(IAuditLogRepository repo, WelfareLinkDbContext context)
        {
            _repo = repo;
            _context = context;
        }

        public async Task<IEnumerable<AuditLog>> GetAllLogsAsync()
            => await _repo.GetAllAsync();

        public async Task<AuditLog?> GetLogByIdAsync(int id)
            => await _repo.GetByIdAsync(id);

        public async Task<IEnumerable<AuditLog>> GetLogsByEntityAsync(string entityType, int entityId)
            => await _repo.GetByEntityAsync(entityType, entityId);

        public async Task<AuditLog> CreateLogAsync(int? userId, string action, string entityType, int entityId, string description)
        {
            var log = new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Description = description,
                Status = "Success",
                Timestamp = DateTime.UtcNow
            };
            return await _repo.AddAsync(log);
        }

        /// <summary>
        /// Enhanced logging with detailed change tracking
        /// </summary>
        public async Task LogUserActionAsync(int? userId, string action, string entityType, int? entityId,
            string description, string? oldValue = null, string? newValue = null,
            string? ipAddress = null, string? userAgent = null)
        {
            var log = new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Description = description,
                OldValue = oldValue,
                NewValue = newValue,
                IPAddress = ipAddress,
                UserAgent = userAgent,
                Status = "Success",
                Timestamp = DateTime.UtcNow
            };
            await _repo.AddAsync(log);
        }

        /// <summary>
        /// Log account creation
        /// </summary>
        public async Task LogAccountCreationAsync(int userId, string username, int? createdByUserId = null)
        {
            await LogUserActionAsync(
                createdByUserId,
                "CREATE",
                "User",
                userId,
                $"Account created for user: {username}",
                null,
                $"Username: {username}, Status: Active"
            );
        }

        /// <summary>
        /// Log account deletion
        /// </summary>
        public async Task LogAccountDeletionAsync(int userId, string username, int? deletedByUserId = null)
        {
            await LogUserActionAsync(
                deletedByUserId,
                "DELETE",
                "User",
                userId,
                $"Account deleted: {username}"
            );
        }

        /// <summary>
        /// Log profile edits
        /// </summary>
        public async Task LogProfileEditAsync(int userId, string changes, int? editedByUserId = null)
        {
            await LogUserActionAsync(
                editedByUserId,
                "UPDATE",
                "User",
                userId,
                $"User profile updated",
                null,
                changes
            );
        }

        /// <summary>
        /// Log benefit allocation
        /// </summary>
        public async Task LogAllocationAsync(int benefitID, string action, int? officerID = null)
        {
            var benefit = await _context.Benefits
                .Include(b => b.WelfareApplication)
                .FirstOrDefaultAsync(b => b.BenefitID == benefitID);

            if (benefit != null)
            {
                await LogUserActionAsync(
                    officerID,
                    action.ToUpper(),
                    "Benefit",
                    benefitID,
                    $"Benefit allocation {action.ToLower()}: Rs. {benefit.Amount} for application #{benefit.ApplicationID}",
                    null,
                    $"Status: {benefit.Status}"
                );
            }
        }

        /// <summary>
        /// Log disbursement
        /// </summary>
        public async Task LogDisbursementAsync(int disbursementID, string action, int? officerID = null)
        {
            var disbursement = await _context.Disbursements
                .FirstOrDefaultAsync(d => d.DisbursementID == disbursementID);

            if (disbursement != null)
            {
                await LogUserActionAsync(
                    officerID,
                    action.ToUpper(),
                    "Disbursement",
                    disbursementID,
                    $"Disbursement {action.ToLower()}: Rs. {disbursement.Amount} for benefit #{disbursement.BenefitID}",
                    null,
                    $"Status: {disbursement.Status}"
                );
            }
        }

        /// <summary>
        /// Get comprehensive audit trail
        /// </summary>
        public async Task<List<AuditLog>> GetAuditTrailAsync(int? userId = null, string? entityType = null, 
            DateTime? from = null, DateTime? to = null)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (userId.HasValue)
                query = query.Where(l => l.UserId == userId);

            if (!string.IsNullOrEmpty(entityType))
                query = query.Where(l => l.EntityType == entityType);

            if (from.HasValue)
                query = query.Where(l => l.Timestamp >= from.Value);

            if (to.HasValue)
                query = query.Where(l => l.Timestamp <= to.Value.AddDays(1));

            return await query
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
        }

        /// <summary>
        /// Log citizen application submission
        /// </summary>
        public async Task LogCitizenApplicationAsync(int applicationID, string action, int? citizenID = null, int? officerID = null)
        {
            var application = await _context.WelfareApplications
                .Include(a => a.Program)
                .FirstOrDefaultAsync(a => a.ApplicationID == applicationID);

            if (application != null)
            {
                await LogUserActionAsync(
                    officerID ?? citizenID,
                    action.ToUpper(),
                    "WelfareApplication",
                    applicationID,
                    $"Citizen application {action.ToLower()}: {application.Program?.Title ?? "Unknown Program"}",
                    null,
                    $"Status: {application.Status}, Citizen: {citizenID}"
                );
            }
        }

        /// <summary>
        /// Log program resource entry
        /// </summary>
        public async Task LogProgramResourceEntryAsync(int resourceID, string action, int? enteredByUserId = null)
        {
            var resource = await _context.Resources
                .Include(r => r.Program)
                .FirstOrDefaultAsync(r => r.ResourceID == resourceID);

            if (resource != null)
            {
                await LogUserActionAsync(
                    enteredByUserId,
                    action.ToUpper(),
                    "Resource",
                    resourceID,
                    $"Program resource {action.ToLower()}: {resource.Type} for program {resource.Program?.Title ?? "Unknown"}",
                    null,
                    $"Quantity: {resource.Quantity}, Status: {resource.Status}"
                );
            }
        }

        /// <summary>
        /// Log program creation/modification
        /// </summary>
        public async Task LogProgramEntryAsync(int programID, string action, int? enteredByUserId = null, string? oldValue = null, string? newValue = null)
        {
            var program = await _context.Programs
                .FirstOrDefaultAsync(p => p.ProgramID == programID);

            if (program != null)
            {
                await LogUserActionAsync(
                    enteredByUserId,
                    action.ToUpper(),
                    "Program",
                    programID,
                    $"Program {action.ToLower()}: {program.Title}",
                    oldValue,
                    newValue ?? $"Budget: {program.Budget}, Status: {program.Status}"
                );
            }
        }

        /// <summary>
        /// Get activity summary for a time period
        /// </summary>
        public async Task<ActivitySummary> GetActivitySummaryAsync(DateTime from, DateTime to)
        {
            var logs = await _context.AuditLogs
                .Where(l => l.Timestamp >= from && l.Timestamp <= to.AddDays(1))
                .ToListAsync();

            var summary = new ActivitySummary
            {
                PeriodStart = from,
                PeriodEnd = to,
                TotalActivities = logs.Count,
                UserCreations = logs.Count(l => l.Action == "CREATE" && l.EntityType == "User"),
                UserModifications = logs.Count(l => l.Action == "UPDATE" && l.EntityType == "User"),
                UserDeletions = logs.Count(l => l.Action == "DELETE" && l.EntityType == "User"),
                ProgramEntries = logs.Count(l => l.EntityType == "Program"),
                ResourceEntries = logs.Count(l => l.EntityType == "Resource"),
                ApplicationSubmissions = logs.Count(l => l.EntityType == "WelfareApplication" && l.Action == "CREATE"),
                BenefitAllocations = logs.Count(l => l.EntityType == "Benefit"),
                DisbursementProcessed = logs.Count(l => l.EntityType == "Disbursement"),
                ComplianceActions = logs.Count(l => l.EntityType == "ComplianceRecord")
            };

            return summary;
        }

        /// <summary>
        /// Get all activities for admin/compliance dashboard
        /// </summary>
        public async Task<List<AuditLog>> GetAllActivitiesAsync(DateTime? from = null, DateTime? to = null, int pageNumber = 1, int pageSize = 50)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (from.HasValue)
                query = query.Where(l => l.Timestamp >= from);

            if (to.HasValue)
                query = query.Where(l => l.Timestamp <= to.Value.AddDays(1));

            return await query
                .OrderByDescending(l => l.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }

    /// <summary>
    /// DTO for activity summary
    /// </summary>
    public class ActivitySummary
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public int TotalActivities { get; set; }
        public int UserCreations { get; set; }
        public int UserModifications { get; set; }
        public int UserDeletions { get; set; }
        public int ProgramEntries { get; set; }
        public int ResourceEntries { get; set; }
        public int ApplicationSubmissions { get; set; }
        public int BenefitAllocations { get; set; }
        public int DisbursementProcessed { get; set; }
        public int ComplianceActions { get; set; }
    }
}

