using Microsoft.AspNetCore.Mvc;
using WelfareLink.Interfaces;

namespace WelfareLink.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EligibilityCheckController : ControllerBase
{
    private readonly IEligibilityCheckService _eligibilityCheckService;

    public EligibilityCheckController(IEligibilityCheckService eligibilityCheckService)
    {
        _eligibilityCheckService = eligibilityCheckService;
    }
}
