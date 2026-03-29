using WelfareLink.Models;

namespace WelfareLink.Interfaces;

public interface INotificationRepository : IGenericRepository<Notification>
{
    Task<IEnumerable<Notification>> GetByUserIdAsync(string userId);
    Task<IEnumerable<Notification>> GetByCategoryAsync(string category);
    Task<IEnumerable<Notification>> GetByStatusAsync(string status);
    Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(string userId);
    Task<IEnumerable<Notification>> GetRecentNotificationsAsync(string userId, int days);
    Task<bool> MarkAsReadAsync(string notificationId);
    Task<bool> MarkAllAsReadAsync(string userId);
    Task<int> GetUnreadCountAsync(string userId);
    Task<IEnumerable<Notification>> GetNotificationsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<bool> DeleteOldNotificationsAsync(int daysOld);
}

