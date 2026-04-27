using Microsoft.AspNetCore.Mvc;
using WelfareLink.Operations.API.Interfaces;

namespace WelfareLink.Operations.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationApiController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationApiController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }
    }
}
