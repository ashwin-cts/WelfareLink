using WelfareLink.Interfaces;
using WelfareLink.Models;

namespace WelfareLink.Services;

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
        try
        {
            await _citizenRepository.UpdateAsync(citizen);
            return true;
        }
        catch
        {
            return false;
        }
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

