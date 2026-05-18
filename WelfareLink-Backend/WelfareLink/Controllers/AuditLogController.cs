using Microsoft.AspNetCore.Mvc;
using WelfareLink.Services;

namespace WelfareLink.Controllers
{
    public class AuditLogController : Controller
    {
        private readonly WelfareApiClient _apiClient;

        public AuditLogController(WelfareApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [HttpGet]
        public async Task<IActionResult> SystemLog(int pageNumber = 1, int pageSize = 10)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var response = await _apiClient.GetPagedAuditLogsAsync(pageNumber, pageSize);
                if (response == null)
                {
                    ViewBag.ErrorMessage = "Failed to fetch audit logs.";
                    return View("SystemLog", null);
                }

                return View("SystemLog", response);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error: {ex.Message}";
                return View("SystemLog", null);
            }
        }

        [HttpGet]
        public async Task<IActionResult> FilterByEntityType(string entityType, int pageNumber = 1, int pageSize = 10)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var response = await _apiClient.GetPagedAuditLogsByEntityTypeAsync(entityType, pageNumber, pageSize);
                if (response == null)
                {
                    ViewBag.ErrorMessage = $"Failed to fetch audit logs for entity type: {entityType}";
                    return View("SystemLog", null);
                }

                ViewBag.FilterType = "EntityType";
                ViewBag.FilterValue = entityType;
                return View("SystemLog", response);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error: {ex.Message}";
                return View("SystemLog", null);
            }
        }

        [HttpGet]
        public async Task<IActionResult> FilterByAction(string action, int pageNumber = 1, int pageSize = 10)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var response = await _apiClient.GetPagedAuditLogsByActionAsync(action, pageNumber, pageSize);
                if (response == null)
                {
                    ViewBag.ErrorMessage = $"Failed to fetch audit logs for action: {action}";
                    return View("SystemLog", null);
                }

                ViewBag.FilterType = "Action";
                ViewBag.FilterValue = action;
                return View("SystemLog", response);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error: {ex.Message}";
                return View("SystemLog", null);
            }
        }

        [HttpGet]
        public async Task<IActionResult> FilterByDateRange(DateTime startDate, DateTime endDate, int pageNumber = 1, int pageSize = 10)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var response = await _apiClient.GetPagedAuditLogsByDateRangeAsync(startDate, endDate, pageNumber, pageSize);
                if (response == null)
                {
                    ViewBag.ErrorMessage = "Failed to fetch audit logs for the specified date range.";
                    return View("SystemLog", null);
                }

                ViewBag.FilterType = "DateRange";
                ViewBag.FilterValue = $"{startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}";
                return View("SystemLog", response);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error: {ex.Message}";
                return View("SystemLog", null);
            }
        }
    }
}
