using WelfareLink.BenifitEligiblity.API.Interfaces;
using WelfareLink.BenifitEligiblity.API.Models;

namespace WelfareLink.BenifitEligiblity.API.Services;

public class CitizenService : ICitizenService
{
    private readonly ICitizenRepository _citizenRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CitizenService(ICitizenRepository citizenRepository, IAuditLogService auditLogService, IHttpContextAccessor httpContextAccessor)
    {
        _citizenRepository = citizenRepository;
        _auditLogService = auditLogService;
        _httpContextAccessor = httpContextAccessor;
    }

    private int? GetCurrentUserId()
    {
        return _httpContextAccessor?.HttpContext?.Session.GetInt32("UserId");
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

        var userId = GetCurrentUserId();
        await _auditLogService.LogActionAsync(
            userId: userId,
            action: "Update",
            entityType: "Citizen",
            entityId: citizen.CitizenId,
            description: $"Updated citizen profile for '{citizen.Name}'",
            status: "Success"
        );

        return true;
    }

    public async Task<bool> CreateCitizenProfileAsync(Citizen citizen)
    {
        try
        {
            await _citizenRepository.AddAsync(citizen);

            var userId = GetCurrentUserId();
            await _auditLogService.LogActionAsync(
                userId: userId,
                action: "Create",
                entityType: "Citizen",
                entityId: citizen.CitizenId,
                description: $"Created citizen profile for '{citizen.Name}'",
                status: "Success"
            );

            return true;
        }
        catch
        {
            return false;
        }
    }
}

