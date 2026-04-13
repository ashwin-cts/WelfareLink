using Microsoft.AspNetCore.Mvc;
using WelfareLink.Services;

namespace WelfareLink.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly WelfareApiClient _api;

    public ReportController(WelfareApiClient api)
    {
        _api = api;
    }
}
