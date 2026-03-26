using Microsoft.AspNetCore.Mvc;
using WelfareLink.Interfaces;

namespace WelfareLink.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WelfareProgramController : ControllerBase
{
    private readonly IWelfareProgramService _programService;

    public WelfareProgramController(IWelfareProgramService programService)
    {
        _programService = programService;
    }
}
