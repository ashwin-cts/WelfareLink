using WelfareLink.BenifitEligiblity.API.Models;

namespace WelfareLink.BenifitEligiblity.API.Interfaces;

public interface IWelfareProgramService
{
    Task<IEnumerable<WelfareProgram>> GetAllProgramsAsync();
    Task<WelfareProgram> GetProgramByIdAsync(int id);
    Task AddProgramAsync(WelfareProgram program);
    Task UpdateProgramAsync(WelfareProgram program);
    Task SuspendProgramAsync(int id);
    Task DeleteProgramAsync(int id);
}
