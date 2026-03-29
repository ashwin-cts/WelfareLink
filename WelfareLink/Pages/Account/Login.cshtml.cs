using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WelfareLink.Interfaces;
using WelfareLink.Models;

namespace WelfareLink.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IAuditLogService _auditService;

        public LoginModel(IAuthenticationService authenticationService, IAuditLogService auditService)
        {
            _authenticationService = authenticationService;
            _auditService = auditService;
        }

        [BindProperty]
        public LoginDto Input { get; set; } = new();

        public string? ReturnUrl { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public void OnGet(string? returnUrl = null, string? message = null)
        {
            ReturnUrl = returnUrl ?? "/Dashboard";
            SuccessMessage = message;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var result = await _authenticationService.LoginAsync(Input.Email, Input.Password);

                if (result.IsSuccess && result.User != null)
                {
                    // Set authentication session/cookie here
                    HttpContext.Session.SetString("UserId", result.User.UserID);
                    HttpContext.Session.SetString("UserName", result.User.Name);
                    HttpContext.Session.SetString("UserRole", result.User.Role);
                    HttpContext.Session.SetString("UserEmail", result.User.Email);

                    // Redirect based on role
                    var redirectPage = GetRoleBasedRedirect(result.User.Role);
                    return RedirectToPage(redirectPage);
                }
                else
                {
                    ErrorMessage = result.ErrorMessage ?? "Invalid login attempt.";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred during login. Please try again.";
                // Log the error
                return Page();
            }
        }

        public async Task<IActionResult> OnPostRegisterAsync()
        {
            return RedirectToPage("/Account/Register");
        }

        private string GetRoleBasedRedirect(string role)
        {
            // All users go to the main dashboard which shows role-specific content
            return "/Dashboard";
        }
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }
}