using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WelfareLink.Interfaces;
using WelfareLink.Models;
using WelfareLink.Services.Helpers;

namespace WelfareLink.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthenticationService _authenticationService;

        public RegisterModel(IUserService userService, IUnitOfWork unitOfWork, IAuthenticationService authenticationService)
        {
            _userService = userService;
            _unitOfWork = unitOfWork;
            _authenticationService = authenticationService;
        }

        [BindProperty]
        public UserRegistrationDto Input { get; set; } = new();

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public void OnGet()
        {
            // This page is for citizen self-registration
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Validate password confirmation
            if (Input.Password != Input.ConfirmPassword)
            {
                ErrorMessage = "Password and confirmation password do not match.";
                return Page();
            }

            try
            {
                // Check if email already exists
                if (await _userService.EmailExistsAsync(Input.Email))
                {
                    ErrorMessage = "An account with this email already exists.";
                    return Page();
                }

                // Create new user (self-registration defaults to Citizen role)
                var user = new User
                {
                    UserID = ServiceHelpers.GenerateUserID(),
                    Name = Input.Name,
                    Role = "Citizen", // Self-registration defaults to Citizen
                    Email = Input.Email,
                    Phone = Input.Phone ?? string.Empty
                };

                // Hash the password they provided
                var passwordHash = await _authenticationService.HashPasswordAsync(Input.Password);

                var createdUser = await _userService.CreateUserAsync(user, Input.Password);

                // Store the password hash in the database
                await StorePasswordHashAsync(createdUser.UserID, passwordHash);

                // Additional audit log for self-registration
                await LogSelfRegistrationAsync(createdUser.UserID, createdUser.Name, createdUser.Email);

                SuccessMessage = $"Account created successfully! You can now login with your email and password.";

                // Clear the form
                Input = new();

                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error creating account: {ex.Message}";
                return Page();
            }
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

        // Log self-registration activity
        private async Task LogSelfRegistrationAsync(string userId, string name, string email)
        {
            var auditLog = new AuditLog
            {
                AuditLogID = Guid.NewGuid().ToString(),
                UserID = userId,
                Action = "Self Registration",
                Resource = "Citizen Registration",
                Timestamp = DateTime.UtcNow
            };

            await _unitOfWork.AuditLogs.AddAsync(auditLog);
            await _unitOfWork.SaveAsync();
        }
    }

    public class UserRegistrationDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}