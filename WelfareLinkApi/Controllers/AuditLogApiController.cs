using Microsoft.AspNetCore.Mvc;
using WelfareLinkApi.Interfaces;

namespace WelfareLinkApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditLogApiController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogApiController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }
    }
}
