using WelfareLink.Models;

namespace WelfareLink.Interfaces;

public interface IWelfareProgramService
{
    Task<IEnumerable<WelfareProgram>> GetAllProgramsAsync();
    Task<WelfareProgram> GetProgramByIdAsync(int id);
    Task AddProgramAsync(WelfareProgram program);
    Task UpdateProgramAsync(WelfareProgram program);
    Task DeleteProgramAsync(int id);
}
