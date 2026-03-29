using WelfareLink.Interfaces;
using WelfareLink.Models;
//using WelfareLink.Service;  

namespace WelfareLink.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IUnitOfWork _unitOfWork;

    public AuditLogService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task LogActionAsync(string userId, string action, string resource, string? details = null, string? ipAddress = null)
    {
        var auditLog = new AuditLog
        {
            AuditLogID = Guid.NewGuid().ToString(),
            UserID = userId,
            Action = action,
            Resource = resource,
            Timestamp = DateTime.UtcNow
        };

        // If your AuditLog model has additional properties for details and IP address
        // auditLog.Details = details;
        // auditLog.IPAddress = ipAddress;

        await _unitOfWork.AuditLogs.AddAsync(auditLog);
        await _unitOfWork.SaveAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(AuditLogFilter filter)
    {
        return await _unitOfWork.AuditLogs.GetFilteredLogsAsync(
            filter.UserId,
            filter.Action,
            filter.Resource,
            filter.StartDate,
            filter.EndDate
        );
    }

    public async Task<IEnumerable<AuditLog>> GetUserActivityAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        if (startDate.HasValue && endDate.HasValue)
        {
            return await _unitOfWork.AuditLogs.GetFilteredLogsAsync(
                userId: userId,
                startDate: startDate,
                endDate: endDate
            );
        }

        return await _unitOfWork.AuditLogs.GetByUserIdAsync(userId);
    }

    public async Task<IEnumerable<AuditLog>> GetRecentActivityAsync(int hours = 24)
    {
        return await _unitOfWork.AuditLogs.GetRecentActivityAsync(hours);
    }

    public async Task<IEnumerable<AuditLog>> GetSecurityEventsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var securityEvents = await _unitOfWork.AuditLogs.GetSecurityEventsAsync();

        if (startDate.HasValue || endDate.HasValue)
        {
            securityEvents = securityEvents.Where(e =>
                (!startDate.HasValue || e.Timestamp >= startDate.Value) &&
                (!endDate.HasValue || e.Timestamp <= endDate.Value)
            );
        }

        return securityEvents;
    }

    public async Task<IEnumerable<AuditLog>> GetLoginAttemptsAsync(DateTime startDate, DateTime endDate)
    {
        return await _unitOfWork.AuditLogs.GetLoginAttemptsAsync(startDate, endDate);
    }

    public async Task<int> GetActionCountAsync(string action, DateTime? startDate = null, DateTime? endDate = null)
    {
        return await _unitOfWork.AuditLogs.GetActionCountAsync(action, startDate, endDate);
    }

    public async Task<Dictionary<string, int>> GetActivityStatisticsAsync(DateTime startDate, DateTime endDate)
    {
        var auditLogs = await _unitOfWork.AuditLogs.GetByDateRangeAsync(startDate, endDate);
        var statistics = new Dictionary<string, int>();

        // Count by action type
        var actionGroups = auditLogs.GroupBy(a => a.Action);
        foreach (var group in actionGroups)
        {
            statistics[$"Action_{group.Key}"] = group.Count();
        }

        // Daily activity count
        var dailyGroups = auditLogs.GroupBy(a => a.Timestamp.Date);
        foreach (var group in dailyGroups)
        {
            statistics[$"Daily_{group.Key:yyyy-MM-dd}"] = group.Count();
        }

        // Total activities
        statistics["Total_Activities"] = auditLogs.Count();

        // Unique users
        statistics["Unique_Users"] = auditLogs.Select(a => a.UserID).Distinct().Count();

        return statistics;
    }

    public async Task<IEnumerable<AuditLog>> GetFailedLoginAttemptsAsync(string? userId = null, DateTime? startDate = null)
    {
        var filter = new AuditLogFilter
        {
            UserId = userId,
            Action = "Failed Login",
            StartDate = startDate ?? DateTime.UtcNow.AddDays(-7) // Default to last 7 days
        };

        return await GetAuditLogsAsync(filter);
    }

    public async Task<bool> CleanupOldAuditLogsAsync(int daysToKeep)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
        var oldLogs = await _unitOfWork.AuditLogs.GetByDateRangeAsync(DateTime.MinValue, cutoffDate);

        if (!oldLogs.Any())
        {
            return false;
        }

        // In a real implementation, you might want to archive rather than delete
        foreach (var log in oldLogs)
        {
            await _unitOfWork.AuditLogs.DeleteAsync(log.AuditLogID);
        }

        await _unitOfWork.SaveAsync();
        return true;
    }
}

