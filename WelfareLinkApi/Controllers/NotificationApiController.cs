using Microsoft.AspNetCore.Mvc;
using WelfareLinkApi.Interfaces;

namespace WelfareLinkApi.Controllers
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
