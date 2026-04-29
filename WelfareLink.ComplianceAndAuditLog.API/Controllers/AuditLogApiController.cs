using Microsoft.AspNetCore.Mvc;
using WelfareLink.ComplianceAndAudit.API.Interfaces;
using WelfareLink.ComplianceAndAudit.API.Models;

namespace WelfareLink.ComplianceAndAudit.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditLogApiController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogApiController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
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

        private IEnumerable<AuditLogDto> MapToDtoList(IEnumerable<AuditLog> logs)
        {
            return logs.Select(MapToDto);
        }

        /// <summary>
        /// Get all audit logs
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var logs = await _auditLogService.GetAllAuditLogsAsync();
            return Ok(MapToDtoList(logs));
        }

        /// <summary>
        /// Get audit log by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var log = await _auditLogService.GetAuditLogByIdAsync(id);
            if (log == null) return NotFound();
            return Ok(MapToDto(log));
        }

        /// <summary>
        /// Get paged audit logs
        /// </summary>
        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var (logs, totalCount) = await _auditLogService.GetPagedAuditLogsAsync(pageNumber, pageSize);
            return Ok(new
            {
                Data = MapToDtoList(logs),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }

        /// <summary>
        /// Get paged audit logs by entity type
        /// </summary>
        [HttpGet("paged/entity/{entityType}")]
        public async Task<IActionResult> GetPagedByEntityType(
            string entityType,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var (logs, totalCount) = await _auditLogService.GetPagedAuditLogsByEntityTypeAsync(entityType, pageNumber, pageSize);
            return Ok(new
            {
                Data = MapToDtoList(logs),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }

        /// <summary>
        /// Get paged audit logs by action
        /// </summary>
        [HttpGet("paged/action/{action}")]
        public async Task<IActionResult> GetPagedByAction(
            string action,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var (logs, totalCount) = await _auditLogService.GetPagedAuditLogsByActionAsync(action, pageNumber, pageSize);
            return Ok(new
            {
                Data = MapToDtoList(logs),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }

        /// <summary>
        /// Get paged audit logs by date range
        /// </summary>
        [HttpGet("paged/date-range")]
        public async Task<IActionResult> GetPagedByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var (logs, totalCount) = await _auditLogService.GetPagedAuditLogsByDateRangeAsync(startDate, endDate, pageNumber, pageSize);
            return Ok(new
            {
                Data = MapToDtoList(logs),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }

        /// <summary>
        /// Delete audit log by ID
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _auditLogService.DeleteAuditLogAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Delete old audit logs
        /// </summary>
        [HttpDelete("purge")]
        public async Task<IActionResult> DeleteOldLogs([FromQuery] int daysOld = 90)
        {
            await _auditLogService.DeleteOldAuditLogsAsync(daysOld);
            return Ok(new { Message = $"Audit logs older than {daysOld} days have been deleted." });
        }
    }
}
