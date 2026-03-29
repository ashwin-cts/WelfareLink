using WelfareLink.Models;
namespace WelfareLink.Interfaces;

public interface INotificationService
{
    Task<Notification> CreateNotificationAsync(string userId, string message, string category, string? subject = null, string? entityId = null);
    Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId);
    Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(string userId);
    Task<int> GetUnreadCountAsync(string userId);
    Task<bool> MarkAsReadAsync(string notificationId);
    Task<bool> MarkAllAsReadAsync(string userId);
    Task<bool> DeleteNotificationAsync(string notificationId);
    Task<IEnumerable<Notification>> GetNotificationsByCategoryAsync(string category);
    Task<IEnumerable<Notification>> GetRecentNotificationsAsync(string userId, int days = 7);
    Task<bool> SendWelcomeNotificationAsync(string userId);
    Task<bool> SendPasswordResetNotificationAsync(string userId);
    Task<bool> SendAccountSuspendedNotificationAsync(string userId, string reason);
    Task<bool> SendAccountActivatedNotificationAsync(string userId);
    Task<bool> SendFailedLoginNotificationAsync(string userId, string ipAddress);
    Task<bool> CleanupOldNotificationsAsync(int daysToKeep = 30);
    Task<Dictionary<string, int>> GetNotificationStatisticsAsync(string userId);
}

