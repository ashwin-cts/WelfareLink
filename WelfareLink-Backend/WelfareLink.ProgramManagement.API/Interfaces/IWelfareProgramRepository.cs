using WelfareLink.ProgramManagement.API.Models;

namespace WelfareLink.ProgramManagement.API.Interfaces;

public interface IWelfareProgramRepository 
{
    Task<IEnumerable<WelfareProgram>> GetAllProgramsAsync();
    Task<WelfareProgram> GetProgramByIdAsync(int id);
    Task AddProgramAsync(WelfareProgram program);
    Task UpdateProgramAsync(WelfareProgram program);
    Task UpdateStatusAsync(int id, string status);
    Task DeleteProgramAsync(int id);
}

