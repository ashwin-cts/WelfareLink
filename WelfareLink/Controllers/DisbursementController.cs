using Microsoft.AspNetCore.Mvc;
using WelfareLink.Interfaces;

namespace WelfareLink.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DisbursementController : ControllerBase
{
    private readonly IDisbursementService _disbursementService;

    public DisbursementController(IDisbursementService disbursementService)
    {
        _disbursementService = disbursementService;
    }
}
