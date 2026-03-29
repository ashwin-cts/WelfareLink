using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WelfareLink.Interfaces;
using WelfareLink.Models;
using WelfareLink.Services.Helpers;

namespace WelfareLink.Pages.Admin
{
    public class UserManagementModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IAuditLogService _auditService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IUnitOfWork _unitOfWork;

        public UserManagementModel(IUserService userService, IAuditLogService auditService, IAuthenticationService authenticationService, IUnitOfWork unitOfWork)
        {
            _userService = userService;
            _auditService = auditService;
            _authenticationService = authenticationService;
            _unitOfWork = unitOfWork;
        }

        [BindProperty]
        public AdminUserRegistrationDto Input { get; set; } = new();

        public List<User> Users { get; set; } = new();
        public string? SearchTerm { get; set; }
        public string? RoleFilter { get; set; }
        public string? StatusFilter { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string? search = null, string? role = null, string? status = null)
        {
            // Check if user is Admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                return RedirectToPage("/Account/AccessDenied");
            }

            SearchTerm = search;
            RoleFilter = role;
            StatusFilter = status;

            if (!string.IsNullOrEmpty(search))
            {
                Users = (await _userService.SearchUsersAsync(search, role, status)).ToList();
            }
            else if (!string.IsNullOrEmpty(role))
            {
                Users = (await _userService.GetUsersByRoleAsync(role)).ToList();
            }
            else
            {
                Users = (await _userService.GetAllUsersAsync()).ToList();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            // Check if user is Admin
            var userRole = HttpContext.Session.GetString("UserRole");
            var currentUserId = HttpContext.Session.GetString("UserId") ?? "";
            var currentUserName = HttpContext.Session.GetString("UserName") ?? "";

            if (userRole != "Admin")
            {
                return RedirectToPage("/Account/AccessDenied");
            }

            if (!ModelState.IsValid)
            {
                await LoadUsersAsync();
                return Page();
            }

            try
            {
                // Check if email already exists
                if (await _userService.EmailExistsAsync(Input.Email))
                {
                    ErrorMessage = "An account with this email already exists.";
                    await LoadUsersAsync();
                    return Page();
                }

                // Create new user
                var user = new User
                {
                    UserID = ServiceHelpers.GenerateUserID(),
                    Name = Input.Name,
                    Role = Input.Role,
                    Email = Input.Email,
                    Phone = Input.Phone ?? string.Empty
                };

                var temporaryPassword = Input.TemporaryPassword ?? ServiceHelpers.GenerateTemporaryPassword();
                var createdUser = await _userService.CreateUserAsync(user, temporaryPassword);

                // Store the password hash for admin-created users too
                var passwordHash = await _authenticationService.HashPasswordAsync(temporaryPassword);
                await StorePasswordHashAsync(createdUser.UserID, passwordHash);

                // Additional audit log for admin-created user
                await _auditService.LogActionAsync(
                    currentUserId, 
                    "Admin Create User", 
                    "User Management", 
                    $"Admin {currentUserName} created user {createdUser.Name} with role {createdUser.Role}"
                );

                SuccessMessage = $"User {createdUser.Name} created successfully with role {createdUser.Role}.";

                // Clear the form
                Input = new();
                await LoadUsersAsync();

                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error creating user: {ex.Message}";
                await LoadUsersAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostSuspendAsync(string userId, string reason)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var currentUserId = HttpContext.Session.GetString("UserId") ?? "";
            var currentUserName = HttpContext.Session.GetString("UserName") ?? "";

            if (userRole != "Admin")
            {
                return new JsonResult(new { success = false, message = "Access denied" });
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return new JsonResult(new { success = false, message = "User not found" });
            }

            var result = await _userService.SuspendUserAsync(userId, reason);
            if (result)
            {
                // Additional audit log for admin action
                await _auditService.LogActionAsync(
                    currentUserId, 
                    "Admin Suspend User", 
                    "User Management", 
                    $"Admin {currentUserName} suspended user {user.Name}. Reason: {reason}"
                );

                return new JsonResult(new { success = true, message = "User suspended successfully" });
            }

            return new JsonResult(new { success = false, message = "Failed to suspend user" });
        }

        public async Task<IActionResult> OnPostActivateAsync(string userId)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var currentUserId = HttpContext.Session.GetString("UserId") ?? "";
            var currentUserName = HttpContext.Session.GetString("UserName") ?? "";

            if (userRole != "Admin")
            {
                return new JsonResult(new { success = false, message = "Access denied" });
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return new JsonResult(new { success = false, message = "User not found" });
            }

            var result = await _userService.ActivateUserAsync(userId);
            if (result)
            {
                // Additional audit log for admin action
                await _auditService.LogActionAsync(
                    currentUserId, 
                    "Admin Activate User", 
                    "User Management", 
                    $"Admin {currentUserName} activated user {user.Name}"
                );

                return new JsonResult(new { success = true, message = "User activated successfully" });
            }

            return new JsonResult(new { success = false, message = "Failed to activate user" });
        }

        private async Task LoadUsersAsync()
        {
            Users = (await _userService.GetAllUsersAsync()).ToList();
        }

        // Store password hash using shadow property
        private async Task StorePasswordHashAsync(string userId, string passwordHash)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user != null)
            {
                var unitOfWork = (WelfareLink.Repositories.UnitOfWork)_unitOfWork;
                unitOfWork.Context.Entry(user).Property("PasswordHash").CurrentValue = passwordHash;
                unitOfWork.Context.Entry(user).Property("Status").CurrentValue = "Active";
                unitOfWork.Context.Entry(user).Property("CreatedDate").CurrentValue = DateTime.UtcNow;
                await unitOfWork.Context.SaveChangesAsync();
            }
        }
    }

    public class AdminUserRegistrationDto
    {
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Status { get; set; } = "Active";
        public string? TemporaryPassword { get; set; }
    }
}