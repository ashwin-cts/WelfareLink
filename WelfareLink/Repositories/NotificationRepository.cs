using WelfareLink.Interfaces;
using WelfareLink.Models;
using WelfareLink.Data;
using Microsoft.EntityFrameworkCore;

namespace WelfareLink.Repositories;

public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
{
    public NotificationRepository(WelfareLinkDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(string userId)
    {
        return await _dbSet
            .Where(n => n.UserID == userId)
            .OrderByDescending(n => n.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetByCategoryAsync(string category)
    {
        return await _dbSet
            .Where(n => n.Category == category)
            .OrderByDescending(n => n.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetByStatusAsync(string status)
    {
        return await _dbSet
            .Where(n => n.Status == status)
            .OrderByDescending(n => n.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(string userId)
    {
        return await _dbSet
            .Where(n => n.UserID == userId && n.Status == "Unread")
            .OrderByDescending(n => n.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetRecentNotificationsAsync(string userId, int days)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        return await _dbSet
            .Where(n => n.UserID == userId && n.CreatedDate >= cutoffDate)
            .OrderByDescending(n => n.CreatedDate)
            .ToListAsync();
    }

    public async Task<bool> MarkAsReadAsync(string notificationId)
    {
        var notification = await GetByIdAsync(notificationId);
        if (notification == null)
            return false;

        notification.Status = "Read";
        return true;
    }

    public async Task<bool> MarkAllAsReadAsync(string userId)
    {
        var unreadNotifications = await _dbSet
            .Where(n => n.UserID == userId && n.Status == "Unread")
            .ToListAsync();

        foreach (var notification in unreadNotifications)
        {
            notification.Status = "Read";
        }

        return unreadNotifications.Any();
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        return await _dbSet
            .CountAsync(n => n.UserID == userId && n.Status == "Unread");
    }

    public async Task<IEnumerable<Notification>> GetNotificationsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(n => n.CreatedDate >= startDate && n.CreatedDate <= endDate)
            .OrderByDescending(n => n.CreatedDate)
            .ToListAsync();
    }

    public async Task<bool> DeleteOldNotificationsAsync(int daysOld)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
        var oldNotifications = await _dbSet
            .Where(n => n.CreatedDate < cutoffDate)
            .ToListAsync();

        if (!oldNotifications.Any())
            return false;

        _dbSet.RemoveRange(oldNotifications);
        return true;
    }
}

