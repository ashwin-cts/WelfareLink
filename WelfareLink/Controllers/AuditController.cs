// using Microsoft.AspNetCore.Mvc;
// using WelfareLink.Interfaces;

// namespace WelfareLink.Controllers;

// [ApiController]
// [Route("api/[controller]")]
// public class AuditController : ControllerBase
// {
//     private readonly IAuditService _auditService;

//     public AuditController(IAuditService auditService)
//     {
//         _auditService = auditService;
//     }
// }
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WelfareLink.Interfaces;
using WelfareLink.Models;

namespace WelfareLink.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _auditService.GetAllAsync();
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var item = await _auditService.GetByIdAsync(id);
        if (item is null) return NotFound();
        return Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Audit audit)
    {
        await _auditService.CreateAsync(audit);
        return CreatedAtAction(nameof(GetById), new { id = audit.AuditID }, audit);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] Audit audit)
    {
        if (id != audit.AuditID) return BadRequest();

        var existing = await _auditService.GetByIdAsync(id);
        if (existing is null) return NotFound();

        await _auditService.UpdateAsync(audit);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var existing = await _auditService.GetByIdAsync(id);
        if (existing is null) return NotFound();

        await _auditService.DeleteAsync(id);
        return NoContent();
    }
}
