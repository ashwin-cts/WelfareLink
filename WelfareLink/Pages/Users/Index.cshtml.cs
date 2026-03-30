using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WelfareLink.Interfaces;
using WelfareLink.Models;

namespace WelfareLink.Pages.Users
{
    public class IndexModel : PageModel
    {
        private readonly IUserService _userService;

        public List<User> Users { get; set; } = new List<User>();

        public IndexModel(IUserService userService)
        {
            _userService = userService;
        }

        public async Task OnGetAsync()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                Users = users?.OrderBy(u => u.Name).ToList() ?? new List<User>();
            }
            catch
            {
                Users = new List<User>();
            }
        }
    }
}
