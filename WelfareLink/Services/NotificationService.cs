using WelfareLink.Interfaces;
using WelfareLink.Models;

namespace WelfareLink.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;

    public NotificationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Notification> CreateNotificationAsync(string userId, string message, string category, string? subject = null, string? entityId = null)
    {
        var notification = new Notification
        {
            NotificationID = Guid.NewGuid().ToString(),
            UserID = userId,
            Message = message,
            Category = category,
            Status = "Unread",
            CreatedDate = DateTime.UtcNow,
            EntityID = entityId ?? string.Empty
        };

        // If your Notification model has a Subject property
        // notification.Subject = subject;

        await _unitOfWork.Notifications.AddAsync(notification);
        await _unitOfWork.SaveAsync();

        return notification;
    }

    public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId)
    {
        return await _unitOfWork.Notifications.GetByUserIdAsync(userId);
    }

    public async Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(string userId)
    {
        return await _unitOfWork.Notifications.GetUnreadNotificationsAsync(userId);
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        return await _unitOfWork.Notifications.GetUnreadCountAsync(userId);
    }

    public async Task<bool> MarkAsReadAsync(string notificationId)
    {
        var result = await _unitOfWork.Notifications.MarkAsReadAsync(notificationId);
        if (result)
        {
            await _unitOfWork.SaveAsync();
        }
        return result;
    }

    public async Task<bool> MarkAllAsReadAsync(string userId)
    {
        var result = await _unitOfWork.Notifications.MarkAllAsReadAsync(userId);
        if (result)
        {
            await _unitOfWork.SaveAsync();
        }
        return result;
    }

    public async Task<bool> DeleteNotificationAsync(string notificationId)
    {
        var result = await _unitOfWork.Notifications.DeleteAsync(notificationId);
        if (result)
        {
            await _unitOfWork.SaveAsync();
        }
        return result;
    }

    public async Task<IEnumerable<Notification>> GetNotificationsByCategoryAsync(string category)
    {
        return await _unitOfWork.Notifications.GetByCategoryAsync(category);
    }

    public async Task<IEnumerable<Notification>> GetRecentNotificationsAsync(string userId, int days = 7)
    {
        return await _unitOfWork.Notifications.GetRecentNotificationsAsync(userId, days);
    }

    public async Task<bool> SendWelcomeNotificationAsync(string userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        var message = $"Welcome to WelfareLink, {user.Name}! Your account has been successfully created.";
        await CreateNotificationAsync(userId, message, "AccountCreated", "Welcome to WelfareLink");

        return true;
    }

    public async Task<bool> SendPasswordResetNotificationAsync(string userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        var message = "A password reset has been requested for your account. If this wasn't you, please contact support immediately.";
        await CreateNotificationAsync(userId, message, "PasswordReset", "Password Reset Request");

        return true;
    }

    public async Task<bool> SendAccountSuspendedNotificationAsync(string userId, string reason)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        var message = $"Your account has been suspended. Reason: {reason}. Please contact an administrator for more information.";
        await CreateNotificationAsync(userId, message, "AccountSuspended", "Account Suspended");

        return true;
    }

    public async Task<bool> SendAccountActivatedNotificationAsync(string userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        var message = "Your account has been activated and you can now access all system features.";
        await CreateNotificationAsync(userId, message, "SystemAlert", "Account Activated");

        return true;
    }

    public async Task<bool> SendFailedLoginNotificationAsync(string userId, string ipAddress)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        var message = $"Multiple failed login attempts detected from IP address: {ipAddress}. If this wasn't you, please secure your account immediately.";
        await CreateNotificationAsync(userId, message, "LoginFailure", "Security Alert");

        return true;
    }

    public async Task<bool> CleanupOldNotificationsAsync(int daysToKeep = 30)
    {
        var result = await _unitOfWork.Notifications.DeleteOldNotificationsAsync(daysToKeep);
        if (result)
        {
            await _unitOfWork.SaveAsync();
        }
        return result;
    }

    public async Task<Dictionary<string, int>> GetNotificationStatisticsAsync(string userId)
    {
        var notifications = await GetUserNotificationsAsync(userId);
        var statistics = new Dictionary<string, int>();

        // Count by status
        statistics["Total_Notifications"] = notifications.Count();
        statistics["Unread_Notifications"] = notifications.Count(n => n.Status == "Unread");
        statistics["Read_Notifications"] = notifications.Count(n => n.Status == "Read");

        // Count by category
        var categoryGroups = notifications.GroupBy(n => n.Category);
        foreach (var group in categoryGroups)
        {
            statistics[$"Category_{group.Key}"] = group.Count();
        }

        // Recent notifications (last 7 days)
        var recentDate = DateTime.UtcNow.AddDays(-7);
        statistics["Recent_Notifications"] = notifications.Count(n => n.CreatedDate >= recentDate);

        return statistics;
    }
}
