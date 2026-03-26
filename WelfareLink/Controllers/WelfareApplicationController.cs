using Microsoft.AspNetCore.Mvc;
using WelfareLink.Interfaces;

namespace WelfareLink.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WelfareApplicationController : ControllerBase
{
    private readonly IWelfareApplicationService _welfareApplicationService;

    public WelfareApplicationController(IWelfareApplicationService welfareApplicationService)
    {
        _welfareApplicationService = welfareApplicationService;
    }
}
