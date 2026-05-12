using System.Security.Claims;
using WelfareLink.CitizenManagement.API.Interfaces;
using WelfareLink.CitizenManagement.API.Models;
using WelfareLink.CitizenManagement.API.DTOs;

namespace WelfareLink.CitizenManagement.API.Services
{
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
            var userIdClaim = _httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? _httpContextAccessor?.HttpContext?.User?.FindFirst("UserId")?.Value;

            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            return null;
        }

        public async Task<Citizen> GetCitizenByIdAsync(int citizenId)
        {
            return await _citizenRepository.GetByIdAsync(citizenId);
        }

        public async Task<Citizen> GetCitizenByUserIdAsync(int userId)
        {
            return await _citizenRepository.GetByUserIdAsync(userId);
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

        // --- CHANGED FROM Citizen to UpdateCitizenDto ---
        public async Task<bool> UpdateCitizenProfileAsync(UpdateCitizenDto dto)
        {
            // Pass the DTO straight to the Repository
            await _citizenRepository.UpdateAsync(dto);

            var userId = GetCurrentUserId();
            await _auditLogService.LogActionAsync(
                userId: userId,
                action: "Update",
                entityType: "Citizen",
                entityId: dto.CitizenId, // Grab ID from DTO
                description: $"Updated citizen profile for '{dto.Name}'", // Grab Name from DTO
                status: "Success"
            );

            return true;
        }
    }
}