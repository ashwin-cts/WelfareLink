using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WelfareLink.Interfaces;

namespace WelfareLink.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly IAuthenticationService _authenticationService;

        public LogoutModel(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userId))
            {
                await _authenticationService.LogoutAsync(userId);
            }

            // Clear session
            HttpContext.Session.Clear();

            return RedirectToPage("/Account/Login", new { message = "You have been logged out successfully." });
        }

        public async Task<IActionResult> OnPostAsync()
        {
            return await OnGetAsync();
        }
    }
}