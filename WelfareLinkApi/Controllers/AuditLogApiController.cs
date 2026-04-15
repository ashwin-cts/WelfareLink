using Microsoft.AspNetCore.Mvc;
using WelfareLinkApi.Interfaces;
using WelfareLinkApi.Models;
using WelfareLinkApi.Data;
using Microsoft.EntityFrameworkCore;

namespace WelfareLinkApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditLogApiController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;
        private readonly IAuditLogServiceEnhanced? _auditLogServiceEnhanced;
        private readonly WelfareLinkDbContext _context;

        public AuditLogApiController(
            IAuditLogService auditLogService,
            WelfareLinkDbContext context,
            IAuditLogServiceEnhanced? auditLogServiceEnhanced = null)
        {
            _auditLogService = auditLogService;
            _context = context;
            _auditLogServiceEnhanced = auditLogServiceEnhanced;
        }

        // GET: api/auditlogapi
        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _auditLogService.GetAllLogsAsync());

        // GET: api/auditlogapi/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var log = await _auditLogService.GetLogByIdAsync(id);
            return log == null ? NotFound() : Ok(log);
        }

        // GET: api/auditlogapi/entity/{type}/{entityId}
        [HttpGet("entity/{entityType}/{entityId}")]
        public async Task<IActionResult> GetByEntity(string entityType, int entityId)
            => Ok(await _auditLogService.GetLogsByEntityAsync(entityType, entityId));

        // GET: api/auditlogapi/filter
        /// <summary>
        /// Get audit logs filtered by criteria
        /// </summary>
        [HttpGet("filter")]
        public async Task<IActionResult> GetFilteredAuditLogs(
            [FromQuery] int? userId = null,
            [FromQuery] string? entityType = null,
            [FromQuery] string? action = null,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (userId.HasValue)
                query = query.Where(a => a.UserId == userId);

            if (!string.IsNullOrEmpty(entityType))
                query = query.Where(a => a.EntityType == entityType);

            if (!string.IsNullOrEmpty(action))
                query = query.Where(a => a.Action == action);

            if (from.HasValue)
                query = query.Where(a => a.Timestamp >= from.Value);

            if (to.HasValue)
                query = query.Where(a => a.Timestamp <= to.Value);

            var logs = await query
                .Include(a => a.User)
                .OrderByDescending(a => a.Timestamp)
                .Select(a => new
                {
                    a.LogID,
                    a.UserId,
                    Username = a.User != null ? a.User.Username : "System",
                    a.Action,
                    a.EntityType,
                    a.EntityId,
                    a.Description,
                    a.OldValue,
                    a.NewValue,
                    a.Status,
                    a.Timestamp,
                    a.IPAddress,
                    a.UserAgent
                })
                .ToListAsync();

            return Ok(logs);
        }

        // GET: api/auditlogapi/user/{userId}
        /// <summary>
        /// Get all audit logs for a specific user
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserAuditTrail(int userId, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
        {
            var query = _context.AuditLogs
                .Where(a => a.UserId == userId)
                .AsQueryable();

            if (from.HasValue)
                query = query.Where(a => a.Timestamp >= from.Value);

            if (to.HasValue)
                query = query.Where(a => a.Timestamp <= to.Value);

            var logs = await query
                .OrderByDescending(a => a.Timestamp)
                .Select(a => new
                {
                    a.LogID,
                    a.Action,
                    a.EntityType,
                    a.Description,
                    a.OldValue,
                    a.NewValue,
                    a.Status,
                    a.Timestamp
                })
                .ToListAsync();

            return Ok(logs);
        }

        // GET: api/auditlogapi/summary
        /// <summary>
        /// Get audit trail summary/statistics
        /// </summary>
        [HttpGet("summary")]
        public async Task<IActionResult> GetAuditSummary(
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (from.HasValue)
                query = query.Where(a => a.Timestamp >= from.Value);

            if (to.HasValue)
                query = query.Where(a => a.Timestamp <= to.Value);

            var actionCounts = await query
                .GroupBy(a => a.Action)
                .Select(g => new { Action = g.Key, Count = g.Count() })
                .ToListAsync();

            var entityCounts = await query
                .GroupBy(a => a.EntityType)
                .Select(g => new { EntityType = g.Key, Count = g.Count() })
                .ToListAsync();

            var userCounts = await query
                .GroupBy(a => a.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToListAsync();

            var total = await query.CountAsync();
            var successCount = await query.Where(a => a.Status == "Success").CountAsync();
            var failureCount = await query.Where(a => a.Status != "Success").CountAsync();

            return Ok(new
            {
                Total = total,
                ByAction = actionCounts,
                ByEntityType = entityCounts,
                ByUser = userCounts,
                Success = successCount,
                Failure = failureCount
            });
        }

        // POST: api/auditlogapi
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAuditLogRequest req)
        {
            var log = await _auditLogService.CreateLogAsync(req.UserId, req.Action, req.EntityType, req.EntityId, req.Description);
            return CreatedAtAction(nameof(GetById), new { id = log.LogID }, log);
        }
    }

    public record CreateAuditLogRequest(int? UserId, string Action, string EntityType, int EntityId, string Description);
}
