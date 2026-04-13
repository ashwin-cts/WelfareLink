using WelfareLinkApi.Interfaces;
using WelfareLinkApi.Models;

namespace WelfareLinkApi.Services;

public class CitizenService : ICitizenService
{
    private readonly ICitizenRepository _citizenRepository;

    public CitizenService(ICitizenRepository citizenRepository)
    {
        _citizenRepository = citizenRepository;
    }

    public async Task<Citizen> GetCitizenByIdAsync(int citizenId)
    {
        return await _citizenRepository.GetByIdAsync(citizenId);
    }

    public async Task<Citizen> GetCitizenByUserIdAsync(int userId)
    {
        return await _citizenRepository.GetByUserIdAsync(userId);
    }

    public async Task<bool> UpdateCitizenProfileAsync(Citizen citizen)
    {
        await _citizenRepository.UpdateAsync(citizen);
        return true;
    }

    public async Task<bool> CreateCitizenProfileAsync(Citizen citizen)
    {
        try
        {
            await _citizenRepository.AddAsync(citizen);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

