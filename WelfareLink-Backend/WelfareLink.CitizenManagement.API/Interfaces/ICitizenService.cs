using WelfareLink.CitizenManagement.API.DTOs;
using WelfareLink.CitizenManagement.API.Models;

namespace WelfareLink.CitizenManagement.API.Interfaces;

public interface ICitizenService
{
    Task<Citizen> GetCitizenByIdAsync(int citizenId);
    Task<Citizen> GetCitizenByUserIdAsync(int userId);
    // FIX: Must use UpdateCitizenDto here!
    Task<bool> UpdateCitizenProfileAsync(UpdateCitizenDto dto);
    Task<bool> CreateCitizenProfileAsync(Citizen citizen);
}
