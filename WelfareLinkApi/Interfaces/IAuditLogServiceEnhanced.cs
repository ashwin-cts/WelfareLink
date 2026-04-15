using WelfareLinkApi.Models;
using WelfareLinkApi.Services;

namespace WelfareLinkApi.Interfaces
{
    public interface IAuditLogServiceEnhanced : IAuditLogService
    {
        Task LogUserActionAsync(int? userId, string action, string entityType, int? entityId,
            string description, string? oldValue = null, string? newValue = null,
            string? ipAddress = null, string? userAgent = null);
        Task LogAccountCreationAsync(int userId, string username, int? createdByUserId = null);
        Task LogAccountDeletionAsync(int userId, string username, int? deletedByUserId = null);
        Task LogProfileEditAsync(int userId, string changes, int? editedByUserId = null);
        Task LogAllocationAsync(int benefitID, string action, int? officerID = null);
        Task LogDisbursementAsync(int disbursementID, string action, int? officerID = null);

        // Enhanced logging for comprehensive audit trail
        Task LogCitizenApplicationAsync(int applicationID, string action, int? citizenID = null, int? officerID = null);
        Task LogProgramResourceEntryAsync(int resourceID, string action, int? enteredByUserId = null);
        Task LogProgramEntryAsync(int programID, string action, int? enteredByUserId = null, string? oldValue = null, string? newValue = null);

        // Reporting and analytics
        Task<ActivitySummary> GetActivitySummaryAsync(DateTime from, DateTime to);
        Task<List<AuditLog>> GetAllActivitiesAsync(DateTime? from = null, DateTime? to = null, int pageNumber = 1, int pageSize = 50);
        Task<List<AuditLog>> GetAuditTrailAsync(int? userId = null, string? entityType = null, DateTime? from = null, DateTime? to = null);
    }
}
