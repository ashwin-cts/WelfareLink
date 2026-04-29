using Microsoft.EntityFrameworkCore;
using WelfareLink.CitizenManagement.API.Data;
using WelfareLink.CitizenManagement.API.Interfaces;
using WelfareLink.CitizenManagement.API.Models;

namespace WelfareLink.CitizenManagement.API.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly WelfareLinkDbContext _context;

        public AuditLogRepository(WelfareLinkDbContext context)
        {
            _context = context;
        }

        private AuditLogDto MapToDto(AuditLog log)
        {
            return new AuditLogDto
            {
                LogID = log.LogID,
                UserId = log.UserId,
                UserName = log.User?.FullName ?? log.User?.Username ?? "System",
                Action = log.Action,
                EntityType = log.EntityType,
                EntityId = log.EntityId,
                Description = log.Description,
                OldValue = log.OldValue,
                NewValue = log.NewValue,
                IPAddress = log.IPAddress,
                UserAgent = log.UserAgent,
                Status = log.Status,
                Timestamp = log.Timestamp
            };
        }

        public async Task<IEnumerable<AuditLog>> GetAllAsync()
        {
            return await _context.AuditLogs
                .AsNoTracking()
                .Include(a => a.User)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<AuditLog?> GetByIdAsync(int id)
        {
            return await _context.AuditLogs
                .AsNoTracking()
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.LogID == id);
        }

        public async Task<(IEnumerable<AuditLog> logs, int totalCount)> GetPagedAsync(int pageNumber, int pageSize)
        {
            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Max(1, pageSize);

            var totalCount = await _context.AuditLogs.CountAsync();
            var logs = await _context.AuditLogs
                .AsNoTracking()
                .Include(a => a.User)
                .OrderByDescending(a => a.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (logs, totalCount);
        }

        public async Task<(IEnumerable<AuditLog> logs, int totalCount)> GetPagedByEntityTypeAsync(string entityType, int pageNumber, int pageSize)
        {
            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Max(1, pageSize);

            var query = _context.AuditLogs.Where(a => a.EntityType == entityType);
            var totalCount = await query.CountAsync();
            var logs = await query
                .AsNoTracking()
                .Include(a => a.User)
                .OrderByDescending(a => a.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (logs, totalCount);
        }

        public async Task<(IEnumerable<AuditLog> logs, int totalCount)> GetPagedByActionAsync(string action, int pageNumber, int pageSize)
        {
            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Max(1, pageSize);

            var query = _context.AuditLogs.Where(a => a.Action == action);
            var totalCount = await query.CountAsync();
            var logs = await query
                .AsNoTracking()
                .Include(a => a.User)
                .OrderByDescending(a => a.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (logs, totalCount);
        }

        public async Task<(IEnumerable<AuditLog> logs, int totalCount)> GetPagedByDateRangeAsync(DateTime startDate, DateTime endDate, int pageNumber, int pageSize)
        {
            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Max(1, pageSize);

            var query = _context.AuditLogs.Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate);
            var totalCount = await query.CountAsync();
            var logs = await query
                .AsNoTracking()
                .Include(a => a.User)
                .OrderByDescending(a => a.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (logs, totalCount);
        }

        public async Task<AuditLog> AddAsync(AuditLog auditLog)
        {
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
            return auditLog;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var auditLog = await _context.AuditLogs.FindAsync(id);
            if (auditLog == null)
            {
                return false;
            }

            _context.AuditLogs.Remove(auditLog);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteOldLogsAsync(int daysOld)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
            var logsToDelete = await _context.AuditLogs
                .Where(a => a.Timestamp < cutoffDate)
                .ToListAsync();

            if (logsToDelete.Count == 0)
                return true;

            _context.AuditLogs.RemoveRange(logsToDelete);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
