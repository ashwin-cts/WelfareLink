using System.Security.Claims; // ADDED THIS FOR JWT CLAIMS
using WelfareLink.AuditorManagement.API.Interfaces;
using WelfareLink.AuditorManagement.API.Models;

namespace WelfareLink.AuditorManagement.API.Services;

public class UserService : IUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuditLogService _auditLogService;

    public UserService(IHttpContextAccessor httpContextAccessor, IAuditLogService auditLogService)
    {
        _httpContextAccessor = httpContextAccessor;
        _auditLogService = auditLogService;
    }

    private int? GetCurrentUserId()
    {
        // Securely extracts the UserId from the JWT Token sent by Postman/Client
        var userIdClaim = _httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? _httpContextAccessor?.HttpContext?.User?.FindFirst("UserId")?.Value;

        if (int.TryParse(userIdClaim, out int userId))
        {
            return userId;
        }

        return null;
    }

    public async Task LogUserCreationAsync(User user)
    {
        var userId = GetCurrentUserId();
        await _auditLogService.LogActionAsync(
            userId: userId,
            action: "Create",
            entityType: "User",
            entityId: user.UserId,
            description: $"Created user '{user.Username}' with role '{user.Role}'",
            status: "Success"
        );
    }

    public async Task LogUserUpdateAsync(User oldUser, User newUser)
    {
        var userId = GetCurrentUserId();
        var changes = new List<string>();

        if (oldUser.FullName != newUser.FullName)
            changes.Add($"FullName: {oldUser.FullName} -> {newUser.FullName}");
        if (oldUser.Role != newUser.Role)
            changes.Add($"Role: {oldUser.Role} -> {newUser.Role}");
        if (oldUser.IsActive != newUser.IsActive)
            changes.Add($"IsActive: {oldUser.IsActive} -> {newUser.IsActive}");

        await _auditLogService.LogActionAsync(
            userId: userId,
            action: "Update",
            entityType: "User",
            entityId: newUser.UserId,
            description: $"Updated user '{newUser.Username}'",
            oldValue: string.Join("; ", changes),
            newValue: string.Join("; ", changes),
            status: "Success"
        );
    }
}