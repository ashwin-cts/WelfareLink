using Microsoft.EntityFrameworkCore;
using WelfareLink.Interfaces;
using WelfareLink.Models;
using WelfareLink.Data;

namespace WelfareLink.Repositories
{

    public class AuditLogRepository : GenericRepository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(WelfareLinkDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(string userId)
        {
            return await _dbSet
                .Where(a => a.UserID == userId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByActionAsync(string action)
        {
            return await _dbSet
                .Where(a => a.Action == action)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByResourceAsync(string resource)
        {
            return await _dbSet
                .Where(a => a.Resource == resource)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetFilteredLogsAsync(string? userId = null, string? action = null, string? resource = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(a => a.UserID == userId);

            if (!string.IsNullOrEmpty(action))
                query = query.Where(a => a.Action == action);

            if (!string.IsNullOrEmpty(resource))
                query = query.Where(a => a.Resource.Contains(resource));

            if (startDate.HasValue)
                query = query.Where(a => a.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.Timestamp <= endDate.Value);

            return await query
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetRecentActivityAsync(int hours)
        {
            var cutoffTime = DateTime.UtcNow.AddHours(-hours);
            return await _dbSet
                .Where(a => a.Timestamp >= cutoffTime)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetUserRecentActivityAsync(string userId, int hours)
        {
            var cutoffTime = DateTime.UtcNow.AddHours(-hours);
            return await _dbSet
                .Where(a => a.UserID == userId && a.Timestamp >= cutoffTime)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<int> GetActionCountAsync(string action, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbSet.Where(a => a.Action == action);

            if (startDate.HasValue)
                query = query.Where(a => a.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.Timestamp <= endDate.Value);

            return await query.CountAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetSecurityEventsAsync()
        {
            var securityActions = new[] { "Login", "Logout", "Failed Login", "Account Suspended", "Password Reset" };

            return await _dbSet
                .Where(a => securityActions.Contains(a.Action))
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetLoginAttemptsAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(a => (a.Action == "Login" || a.Action == "Failed Login") &&
                           a.Timestamp >= startDate && a.Timestamp <= endDate)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }
    }
}

