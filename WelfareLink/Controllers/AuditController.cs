using Microsoft.AspNetCore.Mvc;
using WelfareLink.Services;

namespace WelfareLink.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditController : ControllerBase
{
    private readonly WelfareApiClient _api;

    public AuditController(WelfareApiClient api)
    {
        _api = api;
    }
}
