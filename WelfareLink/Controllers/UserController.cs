using Microsoft.AspNetCore.Mvc;
using WelfareLink.Services;

namespace WelfareLink.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly WelfareApiClient _api;

    public UserController(WelfareApiClient api)
    {
        _api = api;
    }
}
