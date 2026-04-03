// using Microsoft.AspNetCore.Mvc;
// using WelfareLink.Interfaces;

// namespace WelfareLink.Controllers;

// [ApiController]
// [Route("api/[controller]")]
// public class ComplainceRecordController : ControllerBase
// {
//     private readonly IComplainceRecordService _complainceRecordService;

//     public ComplainceRecordController(IComplainceRecordService complainceRecordService)
//     {
//         _complainceRecordService = complainceRecordService;
//     }
// }
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WelfareLink.Interfaces;
using WelfareLink.Models;

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

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _complainceRecordService.GetAllAsync();
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var item = await _complainceRecordService.GetByIdAsync(id);
        if (item is null) return NotFound();
        return Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ComplainceRecord record)
    {
        await _complainceRecordService.CreateAsync(record);
        return CreatedAtAction(nameof(GetById), new { id = record.ComplainceID }, record);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] ComplainceRecord record)
    {
        if (id != record.ComplainceID) return BadRequest();

        var existing = await _complainceRecordService.GetByIdAsync(id);
        if (existing is null) return NotFound();

        await _complainceRecordService.UpdateAsync(record);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var existing = await _complainceRecordService.GetByIdAsync(id);
        if (existing is null) return NotFound();

        await _complainceRecordService.DeleteAsync(id);
        return NoContent();
    }
}
