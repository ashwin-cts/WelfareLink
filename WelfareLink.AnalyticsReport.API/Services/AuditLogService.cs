using WelfareLink.AnalyticsReport.API.Interfaces;
using WelfareLink.AnalyticsReport.API.Models;

namespace WelfareLink.AnalyticsReport.API.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _auditLogRepository;

        public AuditLogService(IAuditLogRepository auditLogRepository)
        {
            _auditLogRepository = auditLogRepository;
        }

        public async Task<IEnumerable<AuditLog>> GetAllAuditLogsAsync()
        {
            return await _auditLogRepository.GetAllAsync();
        }

        public async Task<AuditLog?> GetAuditLogByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Audit Log ID must be greater than zero.", nameof(id));
            }
            return await _auditLogRepository.GetByIdAsync(id);
        }

        public async Task<(IEnumerable<AuditLog> logs, int totalCount)> GetPagedAuditLogsAsync(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0)
            {
                throw new ArgumentException("Page number must be greater than zero.", nameof(pageNumber));
            }
            if (pageSize <= 0)
            {
                throw new ArgumentException("Page size must be greater than zero.", nameof(pageSize));
            }
            return await _auditLogRepository.GetPagedAsync(pageNumber, pageSize);
        }

        public async Task<(IEnumerable<AuditLog> logs, int totalCount)> GetPagedAuditLogsByEntityTypeAsync(string entityType, int pageNumber, int pageSize)
        {
            if (string.IsNullOrWhiteSpace(entityType))
            {
                throw new ArgumentException("Entity type cannot be empty.", nameof(entityType));
            }
            if (pageNumber <= 0)
            {
                throw new ArgumentException("Page number must be greater than zero.", nameof(pageNumber));
            }
            if (pageSize <= 0)
            {
                throw new ArgumentException("Page size must be greater than zero.", nameof(pageSize));
            }
            return await _auditLogRepository.GetPagedByEntityTypeAsync(entityType, pageNumber, pageSize);
        }

        public async Task<(IEnumerable<AuditLog> logs, int totalCount)> GetPagedAuditLogsByActionAsync(string action, int pageNumber, int pageSize)
        {
            if (string.IsNullOrWhiteSpace(action))
            {
                throw new ArgumentException("Action cannot be empty.", nameof(action));
            }
            if (pageNumber <= 0)
            {
                throw new ArgumentException("Page number must be greater than zero.", nameof(pageNumber));
            }
            if (pageSize <= 0)
            {
                throw new ArgumentException("Page size must be greater than zero.", nameof(pageSize));
            }
            return await _auditLogRepository.GetPagedByActionAsync(action, pageNumber, pageSize);
        }

        public async Task<(IEnumerable<AuditLog> logs, int totalCount)> GetPagedAuditLogsByDateRangeAsync(DateTime startDate, DateTime endDate, int pageNumber, int pageSize)
        {
            if (startDate > endDate)
            {
                throw new ArgumentException("Start date must be before or equal to end date.");
            }
            if (pageNumber <= 0)
            {
                throw new ArgumentException("Page number must be greater than zero.", nameof(pageNumber));
            }
            if (pageSize <= 0)
            {
                throw new ArgumentException("Page size must be greater than zero.", nameof(pageSize));
            }
            return await _auditLogRepository.GetPagedByDateRangeAsync(startDate, endDate, pageNumber, pageSize);
        }

        public async Task<AuditLog> LogActionAsync(int? userId, string action, string entityType, int? entityId, string description, string? oldValue = null, string? newValue = null, string? ipAddress = null, string? userAgent = null, string status = "Success")
        {
            if (string.IsNullOrWhiteSpace(action))
            {
                throw new ArgumentException("Action cannot be empty.", nameof(action));
            }
            if (string.IsNullOrWhiteSpace(entityType))
            {
                throw new ArgumentException("Entity type cannot be empty.", nameof(entityType));
            }
            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentException("Description cannot be empty.", nameof(description));
            }

            var auditLog = new AuditLog
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
                Status = status,
                Timestamp = DateTime.UtcNow
            };

            return await _auditLogRepository.AddAsync(auditLog);
        }

        public async Task<bool> DeleteAuditLogAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Audit Log ID must be greater than zero.", nameof(id));
            }
            return await _auditLogRepository.DeleteAsync(id);
        }

        public async Task<bool> DeleteOldAuditLogsAsync(int daysOld)
        {
            if (daysOld <= 0)
            {
                throw new ArgumentException("Days old must be greater than zero.", nameof(daysOld));
            }
            return await _auditLogRepository.DeleteOldLogsAsync(daysOld);
        }
    }
}
