using WelfareLink.CitizenManagement.API.DTOs;
using WelfareLink.CitizenManagement.API.Models;

namespace WelfareLink.CitizenManagement.API.Interfaces;

public interface ICitizenRepository
{
    Task<Citizen> GetByIdAsync(int id);
    Task<Citizen> GetByUserIdAsync(int userId); // Changed to int
    Task AddAsync(Citizen citizen);
    // FIX: Changed name from UpdateCitizenProfileAsync to UpdateAsync
    Task UpdateAsync(UpdateCitizenDto dto);


}
