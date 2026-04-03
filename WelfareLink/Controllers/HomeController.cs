using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WelfareLink.Models;
using WelfareLink.Interfaces;
using WelfareLink.ViewModels;

namespace WelfareLink.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAuditService _auditService;
        private readonly IComplainceRecordService _complService;

        public HomeController(IAuditService auditService, IComplainceRecordService complService)
        {
            _auditService = auditService;
            _complService = complService;
        }

        // Show audits and compliance records on the home page
        public async Task<IActionResult> Index()
        {
            var audits = await _auditService.GetAllAsync();
            var records = await _complService.GetAllAsync();

            var model = new DashboardViewModel
            {
                Audits = audits,
                ComplainceRecords = records
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
