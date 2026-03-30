using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using WelfareLink.Interfaces;
using WelfareLink.Models;

namespace WelfareLink.Pages.Compliance
{
    public class CreateModel : PageModel
    {
        private readonly IComplianceRecordService _complianceService;

        [BindProperty]
        [Required(ErrorMessage = "Entity ID is required")]
        [StringLength(36)]
        public string EntityID { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Type is required")]
        [StringLength(30)]
        public string Type { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Result is required")]
        [StringLength(30)]
        public string Result { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; }

        [BindProperty]
        [StringLength(2000)]
        public string Notes { get; set; }

        public CreateModel(IComplianceRecordService complianceService)
        {
            _complianceService = complianceService;
        }

        public void OnGet()
        {
            Date = DateTime.Now;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var record = new ComplianceRecord
            {
                EntityID = EntityID,
                Type = Type,
                Result = Result,
                Date = Date,
                Notes = Notes
            };

            try
            {
                await _complianceService.CreateRecordAsync(record);
                TempData["SuccessMessage"] = "Compliance record created successfully!";
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
