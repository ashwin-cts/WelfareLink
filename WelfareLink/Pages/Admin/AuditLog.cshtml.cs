using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using WelfareLink.Interfaces;
using WelfareLink.Models;
using static WelfareLink.Interfaces.IAuditLogService;

namespace WelfareLink.Pages.Admin
{
    public class AuditLogModel : PageModel
    {
        private readonly IAuditLogService _auditService;
        private readonly IUserService _userService;

        public AuditLogModel(IAuditLogService auditService, IUserService userService)
        {
            _auditService = auditService;
            _userService = userService;
        }

        public List<AuditLog> AuditLogs { get; set; } = new();
        public List<User> Users { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public AuditLogFilter Filter { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            // Check if user has permission to view audit logs
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId") ?? "";
            var userName = HttpContext.Session.GetString("UserName") ?? "";

            if (userRole != "Admin" && userRole != "Compliance" && userRole != "Auditor")
            {
                return RedirectToPage("/Account/AccessDenied");
            }

            // Load users for dropdown
            Users = (await _userService.GetAllUsersAsync()).ToList();

            // Apply filters and load audit logs
            AuditLogs = (await _auditService.GetAuditLogsAsync(Filter)).ToList();

            // Log audit log access
            await _auditService.LogActionAsync(
                userId, 
                "Access", 
                "Audit Log", 
                $"User {userName} ({userRole}) accessed audit logs with filters: " +
                $"User={Filter.UserId}, Action={Filter.Action}, Resource={Filter.Resource}, " +
                $"StartDate={Filter.StartDate}, EndDate={Filter.EndDate}"
            );

            return Page();
        }

        public async Task<IActionResult> OnGetExportAsync()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId") ?? "";
            var userName = HttpContext.Session.GetString("UserName") ?? "";

            if (userRole != "Admin" && userRole != "Compliance" && userRole != "Auditor")
            {
                return Forbid();
            }

            var auditLogs = await _auditService.GetAuditLogsAsync(Filter);

            // Log audit log export
            await _auditService.LogActionAsync(
                userId, 
                "Export", 
                "Audit Log", 
                $"User {userName} ({userRole}) exported {auditLogs.Count()} audit log records"
            );

            // Create CSV content
            var csv = new StringBuilder();
            csv.AppendLine("Timestamp,User ID,Action,Resource,Details");

            foreach (var log in auditLogs)
            {
                csv.AppendLine($"{log.Timestamp:yyyy-MM-dd HH:mm:ss},{log.UserID},{log.Action},{log.Resource},");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"AuditLog_{DateTime.Now:yyyyMMdd}.csv");
        }
    }
}