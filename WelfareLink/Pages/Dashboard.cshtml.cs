using Microsoft.AspNetCore.Mvc.RazorPages;
using WelfareLink.Interfaces;
using WelfareLink.Models;

namespace WelfareLink.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IAuditLogService _auditService;
        private readonly INotificationService _notificationService;
        private readonly IUnitOfWork _unitOfWork;

        public DashboardModel(IUserService userService, IAuditLogService auditService, INotificationService notificationService, IUnitOfWork unitOfWork)
        {
            _userService = userService;
            _auditService = auditService;
            _notificationService = notificationService;
            _unitOfWork = unitOfWork;
        }

        public string UserRole { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int UnreadNotifications { get; set; }
        public Dictionary<string, int> UserStats { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userId = HttpContext.Session.GetString("UserId");
            UserRole = HttpContext.Session.GetString("UserRole") ?? "";
            UserName = HttpContext.Session.GetString("UserName") ?? "";

            if (!string.IsNullOrEmpty(userId))
            {
                UnreadNotifications = await _notificationService.GetUnreadCountAsync(userId);

                // Log dashboard access
                await LogAuditAsync(userId, "Access", "Dashboard", $"User {UserName} accessed dashboard");
            }

            // Load role-specific statistics
            if (UserRole == "Admin" || UserRole == "Manager")
            {
                UserStats = await _userService.GetUserStatisticsAsync();
            }
        }

        // Log dashboard access activity
        private async Task LogAuditAsync(string userId, string action, string resource, string details)
        {
            var auditLog = new AuditLog
            {
                AuditLogID = Guid.NewGuid().ToString(),
                UserID = userId,
                Action = action,
                Resource = resource,
                Timestamp = DateTime.UtcNow
            };

            await _unitOfWork.AuditLogs.AddAsync(auditLog);
            await _unitOfWork.SaveAsync();
        }
    }
}