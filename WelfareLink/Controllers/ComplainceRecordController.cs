using Microsoft.AspNetCore.Mvc;
using WelfareLink.Services;

namespace WelfareLink.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ComplainceRecordController : ControllerBase
{
    private readonly WelfareApiClient _api;

    public ComplainceRecordController(WelfareApiClient api)
    {
        _api = api;
    }
}
