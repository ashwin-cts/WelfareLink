using Microsoft.EntityFrameworkCore;
using WelfareLinkApi.Data;
using WelfareLinkApi.Interfaces;
using WelfareLinkApi.Models;

namespace WelfareLinkApi.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly WelfareLinkDbContext _context;

    public AuditLogRepository(WelfareLinkDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AuditLog>> GetAllAsync()
        => await _context.AuditLogs
            .Include(l => l.User)
            .OrderByDescending(l => l.Timestamp)
            .ToListAsync();

    public async Task<AuditLog?> GetByIdAsync(int id)
        => await _context.AuditLogs
            .Include(l => l.User)
            .FirstOrDefaultAsync(l => l.LogID == id);

    public async Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, int entityId)
        => await _context.AuditLogs
            .Where(l => l.EntityType == entityType && l.EntityId == entityId)
            .OrderByDescending(l => l.Timestamp)
            .ToListAsync();

    public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(int userId)
        => await _context.AuditLogs
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.Timestamp)
            .ToListAsync();

    public async Task<AuditLog> AddAsync(AuditLog log)
    {
        await _context.AuditLogs.AddAsync(log);
        await _context.SaveChangesAsync();
        return log;
    }
}
