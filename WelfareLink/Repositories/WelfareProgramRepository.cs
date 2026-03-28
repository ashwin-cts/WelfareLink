using WelfareLink.Data;
using WelfareLink.Interfaces;
using WelfareLink.Models;
using WelfareLink.Data;
using Microsoft.EntityFrameworkCore;

namespace WelfareLink.Repositories;

public class WelfareProgramRespository : IWelfareProgramRepository
{
    private readonly WelfareLinkDbContext _context;
    public WelfareProgramRespository(WelfareLinkDbContext context)
    {
        _context = context;
    }
    public async Task<IEnumerable<WelfareProgram>> GetAllProgramsAsync()
    {
        return await _context.Programs.ToListAsync();
    }
    public async Task<WelfareProgram> GetProgramByIdAsync(int id)
    {
        return await _context.Programs.AsNoTracking().FirstOrDefaultAsync(p => p.ProgramID == id);
    }
    public async Task AddProgramAsync(WelfareProgram program)
    {
        await _context.Programs.AddAsync(program);
        await _context.SaveChangesAsync();
    }
    public async Task UpdateProgramAsync(WelfareProgram program)
    {
        // Attach the entity and mark it as modified to avoid tracking conflicts
        var existingProgram = await _context.Programs.FindAsync(program.ProgramID);
        if (existingProgram != null)
        {
            // Update only the properties that should be modified
            existingProgram.Title = program.Title;
            existingProgram.Description = program.Description;
            existingProgram.StartDate = program.StartDate;
            existingProgram.EndDate = program.EndDate;
            existingProgram.Budget = program.Budget;
            existingProgram.Status = program.Status;

            await _context.SaveChangesAsync();
        }
    }
    public async Task DeleteProgramAsync(int id)
    {
        var program = await _context.Programs.FindAsync(id);
        if (program != null)
        {
            _context.Programs.Remove(program);
            await _context.SaveChangesAsync();
        }
    }
}
