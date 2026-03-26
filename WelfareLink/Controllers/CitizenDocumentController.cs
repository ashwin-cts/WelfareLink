using Microsoft.AspNetCore.Mvc;
using WelfareLink.Interfaces;

namespace WelfareLink.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CitizenDocumentController : ControllerBase
{
    private readonly ICitizenDocumentService _citizenDocumentService;

    public CitizenDocumentController(ICitizenDocumentService citizenDocumentService)
    {
        _citizenDocumentService = citizenDocumentService;
    }
}
