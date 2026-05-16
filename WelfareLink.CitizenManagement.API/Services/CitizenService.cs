using System.Security.Claims;
using WelfareLink.CitizenManagement.API.Interfaces;
using WelfareLink.CitizenManagement.API.Models;
using WelfareLink.CitizenManagement.API.DTOs;
using WelfareLink.CitizenManagement.API.Exceptions; // Added for custom exceptions

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
            var citizen = await _citizenRepository.GetByIdAsync(citizenId);
            if (citizen == null)
            {
                throw new NotFoundException($"Citizen profile with ID {citizenId} was not found.");
            }
            return citizen;
        }

        public async Task<Citizen> GetCitizenByUserIdAsync(int userId)
        {
            var citizen = await _citizenRepository.GetByUserIdAsync(userId);
            if (citizen == null)
            {
                throw new NotFoundException($"Citizen profile for User ID {userId} was not found.");
            }
            return citizen;
        }

        public async Task<bool> CreateCitizenProfileAsync(Citizen citizen)
        {
            try
            {
                if (citizen == null)
                {
                    throw new BusinessValidationException("Citizen profile data cannot be empty.");
                }

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
            catch (BusinessValidationException)
            {
                throw; // Preserve the validation exception
            }
            catch (Exception)
            {
                throw new BadRequestException("An error occurred while attempting to create the citizen profile.");
            }
        }

        // --- CHANGED FROM Citizen to UpdateCitizenDto ---
        public async Task<bool> UpdateCitizenProfileAsync(UpdateCitizenDto dto)
        {
            try
            {
                if (dto == null)
                {
                    throw new BusinessValidationException("Update data cannot be empty.");
                }

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
            catch (BusinessValidationException)
            {
                throw; // Preserve the validation exception
            }
            catch (Exception)
            {
                throw new BadRequestException("An error occurred while attempting to update the citizen profile.");
            }
        }
    }
}