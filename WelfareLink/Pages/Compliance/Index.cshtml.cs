using Microsoft.AspNetCore.Mvc.RazorPages;
using WelfareLink.Interfaces;
using WelfareLink.Models;

namespace WelfareLink.Pages.Compliance
{
    public class IndexModel : PageModel
    {
        private readonly IComplianceRecordService _complianceService;

        public List<ComplianceRecord> Records { get; set; } = new List<ComplianceRecord>();

        public IndexModel(IComplianceRecordService complianceService)
        {
            _complianceService = complianceService;
        }

        public async Task OnGetAsync()
        {
            try
            {
                var records = await _complianceService.GetAllRecordsAsync();
                Records = records?.OrderByDescending(r => r.Date).ToList() ?? new List<ComplianceRecord>();
            }
            catch
            {
                Records = new List<ComplianceRecord>();
            }
        }
    }
}
