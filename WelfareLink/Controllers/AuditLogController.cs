using Microsoft.AspNetCore.Mvc;
using WelfareLink.Services;

namespace WelfareLink.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditLogController : ControllerBase
{
    private readonly WelfareApiClient _api;

    public AuditLogController(WelfareApiClient api)
    {
        _api = api;
    }
}
