using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using WelfareLink.Interfaces;
using WelfareLink.Models;

namespace WelfareLink.Pages.Audits
{
    public class CreateModel : PageModel
    {
        private readonly IAuditService _auditService;
        private readonly IUserService _userService;

        public List<User> AvailableOfficers { get; set; } = new List<User>();

        [BindProperty]
        [Required(ErrorMessage = "Officer is required")]
        public string OfficerID { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Scope is required")]
        [StringLength(500)]
        public string Scope { get; set; }

        [BindProperty]
        [StringLength(4000)]
        public string Findings { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; }

        public CreateModel(IAuditService auditService, IUserService userService)
        {
            _auditService = auditService;
            _userService = userService;
        }

        public async Task OnGetAsync()
        {
            Date = DateTime.Now;
            try
            {
                var users = await _userService.GetAllUsersAsync();
                AvailableOfficers = users?.ToList() ?? new List<User>();
            }
            catch
            {
                AvailableOfficers = new List<User>();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var users = await _userService.GetAllUsersAsync();
                AvailableOfficers = users?.ToList() ?? new List<User>();
                return Page();
            }

            var audit = new Audit
            {
                OfficerID = OfficerID,
                Scope = Scope,
                Findings = Findings,
                Date = Date,
                Status = Status
            };

            try
            {
                await _auditService.CreateAuditAsync(audit);
                TempData["SuccessMessage"] = "Audit created successfully!";
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                var users = await _userService.GetAllUsersAsync();
                AvailableOfficers = users?.ToList() ?? new List<User>();
                return Page();
            }
        }
    }
}
