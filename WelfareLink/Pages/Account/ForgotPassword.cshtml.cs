using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WelfareLink.Interfaces;

namespace WelfareLink.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly IAuthenticationService _authenticationService;

        public ForgotPasswordModel(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var result = await _authenticationService.ResetPasswordAsync(Email);
                
                if (result)
                {
                    SuccessMessage = "If an account with this email exists, we've sent password reset instructions to your email address.";
                }
                else
                {
                    SuccessMessage = "If an account with this email exists, we've sent password reset instructions to your email address.";
                }

                // Always show success message for security (don't reveal if email exists)
                Email = string.Empty;
                return Page();
            }
            catch (Exception)
            {
                ErrorMessage = "An error occurred while processing your request. Please try again.";
                return Page();
            }
        }
    }
}