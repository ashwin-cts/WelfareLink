using Microsoft.AspNetCore.Mvc;
using WelfareLinkApi.Interfaces;

namespace WelfareLinkApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComplainceRecordApiController : ControllerBase
    {
        private readonly IComplainceRecordService _complainceRecordService;

        public ComplainceRecordApiController(IComplainceRecordService complainceRecordService)
        {
            _complainceRecordService = complainceRecordService;
        }
    }
}
