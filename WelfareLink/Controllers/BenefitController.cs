using Microsoft.AspNetCore.Mvc;
using WelfareLink.Interfaces;

namespace WelfareLink.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BenefitController : ControllerBase
{
    private readonly IBenefitService _benefitService;

    public BenefitController(IBenefitService benefitService)
    {
        _benefitService = benefitService;
    }
}
