using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using WelfareLink.Interfaces;
using WelfareLink.Models;

namespace WelfareLink.Pages.Users
{
    public class CreateModel : PageModel
    {
        private readonly IUserService _userService;

        [BindProperty]
        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "Full Name")]
        public string Name { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; }

        [BindProperty]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string Phone { get; set; }

        public CreateModel(IUserService userService)
        {
            _userService = userService;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = new User
            {
                Name = Name,
                Email = Email,
                Role = Role,
                Phone = Phone
            };

            try
            {
                await _userService.CreateUserAsync(user);
                TempData["SuccessMessage"] = "User created successfully!";
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                return Page();
            }
        }
    }
}
