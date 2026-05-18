using WelfareLink.WelfareOfficerManagement.API.Data;
using WelfareLink.WelfareOfficerManagement.API.Interfaces;
using WelfareLink.WelfareOfficerManagement.API.Models;
//using WelfareLink.WelfareOfficerManagement.API.Data;
using Microsoft.EntityFrameworkCore;

namespace WelfareLink.WelfareOfficerManagement.API.Repositories;

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
            existingProgram.MaxBenefitPerCitizen = program.MaxBenefitPerCitizen;
            existingProgram.Status = program.Status;
            existingProgram.EligibleGender = program.EligibleGender;
            existingProgram.RequiredDocuments = program.RequiredDocuments;

            await _context.SaveChangesAsync();
        }
    }
    public async Task UpdateStatusAsync(int id, string status)
    {
        var program = await _context.Programs.FindAsync(id);
        if (program != null)
        {
            program.Status = status;
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
