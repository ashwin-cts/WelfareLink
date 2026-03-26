using Microsoft.AspNetCore.Mvc;
using WelfareLink.Interfaces;

namespace WelfareLink.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CitizenController : ControllerBase
{
    private readonly ICitizenService _citizenService;

    public CitizenController(ICitizenService citizenService)
    {
        _citizenService = citizenService;
    }
}
