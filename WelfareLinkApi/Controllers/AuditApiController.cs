using Microsoft.AspNetCore.Mvc;
using WelfareLinkApi.Interfaces;

namespace WelfareLinkApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditApiController : ControllerBase
    {
        private readonly IAuditService _auditService;

        public AuditApiController(IAuditService auditService)
        {
            _auditService = auditService;
        }
    }
}
