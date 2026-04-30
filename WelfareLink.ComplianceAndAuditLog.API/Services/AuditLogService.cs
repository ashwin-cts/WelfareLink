using WelfareLink.ComplianceAndAuditLog.API.Exceptions;
using WelfareLink.ComplianceAndAuditLog.API.Interfaces;
using WelfareLink.ComplianceAndAuditLog.API.Models;


namespace WelfareLink.ComplianceAndAuditLog.API.Services
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
                throw new BadRequestException("Audit Log ID must be greater than zero.");
            }
            return await _auditLogRepository.GetByIdAsync(id);
        }

        public async Task<(IEnumerable<AuditLog> logs, int totalCount)> GetPagedAuditLogsAsync(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0)
            {
                throw new BadRequestException("Page number must be greater than zero.");
            }
            if (pageSize <= 0)
            {
                throw new BadRequestException("Page size must be greater than zero.");
            }
            return await _auditLogRepository.GetPagedAsync(pageNumber, pageSize);
        }

        public async Task<(IEnumerable<AuditLog> logs, int totalCount)> GetPagedAuditLogsByEntityTypeAsync(string entityType, int pageNumber, int pageSize)
        {
            if (string.IsNullOrWhiteSpace(entityType))
            {
                throw new BadRequestException("Entity type cannot be empty.");
            }
            if (pageNumber <= 0)
            {
                throw new BadRequestException("Page number must be greater than zero.");
            }
            if (pageSize <= 0)
            {
                throw new BadRequestException("Page size must be greater than zero.");
            }
            return await _auditLogRepository.GetPagedByEntityTypeAsync(entityType, pageNumber, pageSize);
        }

        public async Task<(IEnumerable<AuditLog> logs, int totalCount)> GetPagedAuditLogsByActionAsync(string action, int pageNumber, int pageSize)
        {
            if (string.IsNullOrWhiteSpace(action))
            {
                throw new BadRequestException("Action cannot be empty.");
            }
            if (pageNumber <= 0)
            {
                throw new BadRequestException("Page number must be greater than zero.");
            }
            if (pageSize <= 0)
            {
                throw new BadRequestException("Page size must be greater than zero.");
            }
            return await _auditLogRepository.GetPagedByActionAsync(action, pageNumber, pageSize);
        }

        public async Task<(IEnumerable<AuditLog> logs, int totalCount)> GetPagedAuditLogsByDateRangeAsync(DateTime startDate, DateTime endDate, int pageNumber, int pageSize)
        {
            if (startDate > endDate)
            {
                throw new BadRequestException("Start date must be before or equal to end date.");
            }
            if (pageNumber <= 0)
            {
                throw new BadRequestException("Page number must be greater than zero.");
            }
            if (pageSize <= 0)
            {
                throw new BadRequestException("Page size must be greater than zero.");
            }
            return await _auditLogRepository.GetPagedByDateRangeAsync(startDate, endDate, pageNumber, pageSize);
        }

        public async Task<AuditLog> LogActionAsync(int? userId, string action, string entityType, int? entityId, string description, string? oldValue = null, string? newValue = null, string? ipAddress = null, string? userAgent = null, string status = "Success")
        {
            if (string.IsNullOrWhiteSpace(action))
            {
                throw new BadRequestException("Action cannot be empty.");
            }
            if (string.IsNullOrWhiteSpace(entityType))
            {
                throw new BadRequestException("Entity type cannot be empty.");
            }
            if (string.IsNullOrWhiteSpace(description))
            {
                throw new BadRequestException("Description cannot be empty.");
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
                throw new BadRequestException("Audit Log ID must be greater than zero.");
            }
            return await _auditLogRepository.DeleteAsync(id);
        }

        public async Task<bool> DeleteOldAuditLogsAsync(int daysOld)
        {
            if (daysOld <= 0)
            {
                throw new BadRequestException("Days old must be greater than zero.");
            }
            return await _auditLogRepository.DeleteOldLogsAsync(daysOld);
        }
    }
}