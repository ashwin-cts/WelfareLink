using WelfareLink.Interfaces;
using WelfareLink.Models;

namespace WelfareLink.Services;

public class WelfareProgramService : IWelfareProgramService
{
    private readonly IWelfareProgramRepository _programRepository;

    public WelfareProgramService(IWelfareProgramRepository programRepository)
    {
        _programRepository = programRepository;
    }

    public async Task<IEnumerable<WelfareProgram>> GetAllProgramsAsync()
    {
        return await _programRepository.GetAllProgramsAsync();
    }

    public async Task<WelfareProgram> GetProgramByIdAsync(int id)
    {
        return await _programRepository.GetProgramByIdAsync(id);
    }

    public async Task AddProgramAsync(WelfareProgram program)
    {
        ValidateProgramDates(program, isNewProgram: true);
        ValidateProgramBudget(program);
        await ValidateDuplicateTitle(program.Title, program.ProgramID);

        program.Status = "Active";

        await _programRepository.AddProgramAsync(program);
    }

    public async Task UpdateProgramAsync(WelfareProgram program)
    {
        ValidateProgramDates(program, isNewProgram: false);
        ValidateProgramBudget(program);
        await ValidateDuplicateTitle(program.Title, program.ProgramID);

        await _programRepository.UpdateProgramAsync(program);
    }

    public async Task DeleteProgramAsync(int id)
    {
        var program = await _programRepository.GetProgramByIdAsync(id);
        if (program == null)
        {
            throw new InvalidOperationException($"Program with ID {id} not found.");
        }

        if (program.Status == "Active")
        {
            throw new InvalidOperationException("Cannot delete an active program. Please suspend or complete it first.");
        }

        await _programRepository.DeleteProgramAsync(id);
    }

    private void ValidateProgramDates(WelfareProgram program, bool isNewProgram)
    {
        if (program.EndDate <= program.StartDate)
        {
            throw new InvalidOperationException("Programme end date must be after the start date.");
        }

        // Only check if start date is in the past for NEW programmes
        // Allow updates to existing programmes even if start date is in the past
        if (isNewProgram && program.StartDate < DateTime.Today)
        {
            throw new InvalidOperationException("Programme start date cannot be in the past.");
        }
    }

    private void ValidateProgramBudget(WelfareProgram program)
    {
        if (program.Budget <= 0)
        {
            throw new InvalidOperationException("Programme budget must be greater than zero.");
        }
    }

    private async Task ValidateDuplicateTitle(string title, int programId)
    {
        var existingPrograms = await _programRepository.GetAllProgramsAsync();
        var duplicate = existingPrograms.FirstOrDefault(p =>
            p.Title.Equals(title, StringComparison.OrdinalIgnoreCase) &&
            p.ProgramID != programId);

        if (duplicate != null)
        {
            throw new InvalidOperationException($"A programme with the title '{title}' already exists.");
        }
    }
}
