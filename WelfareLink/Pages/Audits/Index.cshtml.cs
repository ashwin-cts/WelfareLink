using Microsoft.AspNetCore.Mvc.RazorPages;
using WelfareLink.Interfaces;
using WelfareLink.Models;

namespace WelfareLink.Pages.Audits
{
    public class IndexModel : PageModel
    {
        private readonly IAuditService _auditService;

        public List<Audit> Audits { get; set; } = new List<Audit>();

        public IndexModel(IAuditService auditService)
        {
            _auditService = auditService;
        }

        public async Task OnGetAsync()
        {
            try
            {
                var audits = await _auditService.GetAllAuditsAsync();
                Audits = audits?.OrderByDescending(a => a.Date).ToList() ?? new List<Audit>();
            }
            catch
            {
                Audits = new List<Audit>();
            }
        }
    }
}
