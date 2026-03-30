using Microsoft.AspNetCore.Mvc.RazorPages;
using WelfareLink.Interfaces;

namespace WelfareLink.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IAuditService _auditService;
        private readonly IComplianceRecordService _complianceService;
        private readonly IUserService _userService;

        public int TotalAudits { get; set; }
        public int PendingAudits { get; set; }
        public int TotalCompliance { get; set; }
        public int TotalUsers { get; set; }

        public IndexModel(
            IAuditService auditService,
            IComplianceRecordService complianceService,
            IUserService userService)
        {
            _auditService = auditService;
            _complianceService = complianceService;
            _userService = userService;
        }

        public async Task OnGetAsync()
        {
            try
            {
                var audits = await _auditService.GetAllAuditsAsync();
                var compliance = await _complianceService.GetAllRecordsAsync();
                var users = await _userService.GetAllUsersAsync();

                TotalAudits = audits?.Count() ?? 0;
                PendingAudits = audits?.Count(a => a.Status == "Pending") ?? 0;
                TotalCompliance = compliance?.Count() ?? 0;
                TotalUsers = users?.Count() ?? 0;
            }
            catch
            {
                // Set defaults if services fail
                TotalAudits = 0;
                PendingAudits = 0;
                TotalCompliance = 0;
                TotalUsers = 0;
            }
        }
    }
}
