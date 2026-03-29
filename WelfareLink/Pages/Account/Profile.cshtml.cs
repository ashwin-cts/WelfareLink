using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WelfareLink.Interfaces;
using WelfareLink.Models;

namespace WelfareLink.Pages.Account
{
    public class ProfileModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IUnitOfWork _unitOfWork;

        public ProfileModel(IUserService userService, IAuthenticationService authenticationService, IUnitOfWork unitOfWork)
        {
            _userService = userService;
            _authenticationService = authenticationService;
            _unitOfWork = unitOfWork;
        }

        [BindProperty]
        public ProfileDto ProfileInput { get; set; } = new();

        [BindProperty]
        public PasswordChangeDto PasswordInput { get; set; } = new();

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
        public Models.User? CurrentUser { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login");
            }

            CurrentUser = await _userService.GetUserByIdAsync(userId);
            if (CurrentUser == null)
            {
                return RedirectToPage("/Account/Login");
            }

            // Populate profile form
            ProfileInput = new ProfileDto
            {
                Name = CurrentUser.Name,
                Email = CurrentUser.Email,
                Phone = CurrentUser.Phone
            };

            // Log profile view
            await LogAuditAsync(userId, "View", "Profile", $"User {CurrentUser.Name} viewed their profile");

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateProfileAsync()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login");
            }

            if (!ModelState.IsValid)
            {
                CurrentUser = await _userService.GetUserByIdAsync(userId);
                return Page();
            }

            try
            {
                var result = await _userService.UpdateUserProfileAsync(
                    userId, 
                    ProfileInput.Name, 
                    ProfileInput.Email, 
                    ProfileInput.Phone);

                if (result)
                {
                    // Update session data
                    HttpContext.Session.SetString("UserName", ProfileInput.Name);
                    HttpContext.Session.SetString("UserEmail", ProfileInput.Email);

                    SuccessMessage = "Profile updated successfully!";
                }
                else
                {
                    ErrorMessage = "Failed to update profile. Please try again.";
                }

                CurrentUser = await _userService.GetUserByIdAsync(userId);
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error updating profile: {ex.Message}";
                CurrentUser = await _userService.GetUserByIdAsync(userId);
                return Page();
            }
        }

        public async Task<IActionResult> OnPostChangePasswordAsync()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login");
            }

            if (!ModelState.IsValid)
            {
                CurrentUser = await _userService.GetUserByIdAsync(userId);
                return Page();
            }

            try
            {
                var result = await _authenticationService.ChangePasswordAsync(
                    userId, 
                    PasswordInput.CurrentPassword, 
                    PasswordInput.NewPassword);

                if (result)
                {
                    SuccessMessage = "Password changed successfully!";
                    PasswordInput = new(); // Clear password fields
                }
                else
                {
                    ErrorMessage = "Failed to change password. Please check your current password.";
                }

                CurrentUser = await _userService.GetUserByIdAsync(userId);
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error changing password: {ex.Message}";
                CurrentUser = await _userService.GetUserByIdAsync(userId);
                return Page();
            }
        }

        // Log profile-related activities
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

    public class ProfileDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
    }

    public class PasswordChangeDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}