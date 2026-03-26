using Microsoft.AspNetCore.Mvc;
using WelfareLink.Interfaces;

namespace WelfareLink.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ComplainceRecordController : ControllerBase
{
    private readonly IComplainceRecordService _complainceRecordService;

    public ComplainceRecordController(IComplainceRecordService complainceRecordService)
    {
        _complainceRecordService = complainceRecordService;
    }
}
