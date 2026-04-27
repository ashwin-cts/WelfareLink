using WelfareLink.WApplicationSystem.API.Models;

namespace WelfareLink.WApplicationSystem.API.Interfaces;

public interface ICitizenService
{
    Task<Citizen> GetCitizenByIdAsync(int citizenId);
    Task<Citizen> GetCitizenByUserIdAsync(int userId);
    Task<bool> UpdateCitizenProfileAsync(Citizen citizen);
    Task<bool> CreateCitizenProfileAsync(Citizen citizen);
}
