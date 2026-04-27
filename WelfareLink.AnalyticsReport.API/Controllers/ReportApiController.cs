using Microsoft.AspNetCore.Mvc;
using WelfareLink.AnalyticsReport.API.Interfaces;

namespace WelfareLink.AnalyticsReport.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportApiController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportApiController(IReportService reportService)
        {
            _reportService = reportService;
        }
    }
}
