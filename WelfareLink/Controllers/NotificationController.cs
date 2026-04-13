using Microsoft.AspNetCore.Mvc;
using WelfareLink.Services;

namespace WelfareLink.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly WelfareApiClient _api;

    public NotificationController(WelfareApiClient api)
    {
        _api = api;
    }
}
