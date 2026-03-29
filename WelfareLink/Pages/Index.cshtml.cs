using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WelfareLink.Pages
{
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Check if user is already logged in
            var userId = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userId))
            {
                // Redirect to dashboard if already logged in
                return RedirectToPage("/Dashboard");
            }
            
            // Redirect to login page if not logged in
            return RedirectToPage("/Account/Login");
        }
    }
}