using WelfareLink.Interfaces;
using WelfareLink.Models;
//using WelfareLink.Services.ServiceHelpers;
using WelfareLink.Services;
using WelfareLink.Services.Helpers;
namespace WelfareLink.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _unitOfWork.Users.GetAllAsync();
    }

    public async Task<User?> GetUserByIdAsync(string userId)
    {
        return await _unitOfWork.Users.GetByIdAsync(userId);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _unitOfWork.Users.GetByEmailAsync(email);
    }

    public async Task<IEnumerable<User>> GetUsersByRoleAsync(string role)
    {
        return await _unitOfWork.Users.GetByRoleAsync(role);
    }

    public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm, string? roleFilter = null, string? statusFilter = null)
    {
        var users = await _unitOfWork.Users.SearchUsersAsync(searchTerm);

        if (!string.IsNullOrEmpty(roleFilter))
        {
            users = users.Where(u => u.Role == roleFilter);
        }

        if (!string.IsNullOrEmpty(statusFilter))
        {
            // Note: Status filtering would need to be added to repository or handled here
            // users = users.Where(u => u.Status == statusFilter);
        }

        return users;
    }

    public async Task<User> CreateUserAsync(User user, string temporaryPassword)
    {
        // Validate unique email
        if (await EmailExistsAsync(user.Email))
        {
            throw new InvalidOperationException("Email already exists");
        }

        // Generate unique UserID if not provided
        if (string.IsNullOrEmpty(user.UserID))
        {
            user.UserID = ServiceHelpers.GenerateUserID();
        }

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveAsync();

        // Log the user creation activity
        await LogAuditAsync(user.UserID, "Create", "User", $"User {user.Name} created with role {user.Role}");

        return user;
    }

    public async Task<User> UpdateUserAsync(User user)
    {
        var existingUser = await GetUserByIdAsync(user.UserID);
        if (existingUser == null)
        {
            throw new InvalidOperationException("User not found");
        }

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveAsync();

        // Log the user update activity
        await LogAuditAsync(user.UserID, "Update", "User", $"User {user.Name} information updated");

        return user;
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        var user = await GetUserByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        var result = await _unitOfWork.Users.DeleteAsync(userId);
        if (result)
        {
            await _unitOfWork.SaveAsync();
            // Log the user deletion activity
            await LogAuditAsync(userId, "Delete", "User", $"User {user.Name} deleted from system");
        }

        return result;
    }

    public async Task<bool> SuspendUserAsync(string userId, string reason)
    {
        var user = await GetUserByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveAsync();

        // Log the user suspension activity
        await LogAuditAsync(userId, "Suspend", "User", $"User {user.Name} suspended. Reason: {reason}");

        return true;
    }

    public async Task<bool> ActivateUserAsync(string userId)
    {
        var user = await GetUserByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveAsync();

        // Log the user activation activity
        await LogAuditAsync(userId, "Activate", "User", $"User {user.Name} account activated");

        return true;
    }

    public async Task<bool> DeactivateUserAsync(string userId)
    {
        var user = await GetUserByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveAsync();

        // Log the user deactivation activity
        await LogAuditAsync(userId, "Deactivate", "User", $"User {user.Name} account deactivated");

        return true;
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _unitOfWork.Users.EmailExistsAsync(email);
    }

    public async Task<int> GetUserCountByRoleAsync(string role)
    {
        return await _unitOfWork.Users.GetUserCountByRoleAsync(role);
    }

    public async Task<IEnumerable<User>> GetRecentlyCreatedUsersAsync(int days)
    {
        return await _unitOfWork.Users.GetRecentlyCreatedUsersAsync(days);
    }

    public async Task<bool> UpdateUserProfileAsync(string userId, string name, string email, string? phone)
    {
        var user = await GetUserByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        var oldName = user.Name;
        var oldEmail = user.Email;
        var oldPhone = user.Phone;

        user.Name = name;
        user.Email = email;
        user.Phone = phone ?? string.Empty;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveAsync();

        // Log profile update with details
        var changes = new List<string>();
        if (oldName != name) changes.Add($"Name: {oldName} → {name}");
        if (oldEmail != email) changes.Add($"Email: {oldEmail} → {email}");
        if (oldPhone != phone) changes.Add($"Phone: {oldPhone} → {phone}");

        await LogAuditAsync(userId, "Update", "Profile", $"Profile updated. Changes: {string.Join(", ", changes)}");

        return true;
    }

    // Private method to create audit logs without circular dependency
    private async Task LogAuditAsync(string userId, string action, string resource, string details)
    {
        var auditLog = new Models.AuditLog
        {
            AuditLogID = Guid.NewGuid().ToString(),
            UserID = userId,
            Action = action,
            Resource = resource,
            Timestamp = DateTime.UtcNow
        };

        await _unitOfWork.AuditLogs.AddAsync(auditLog);
        // Note: SaveAsync is called in the main methods to avoid multiple saves
    }

    public async Task<Dictionary<string, int>> GetUserStatisticsAsync()
    {
        var allUsers = await GetAllUsersAsync();
        var statistics = new Dictionary<string, int>();

        // Count by role
        var roles = new[] { "Citizen", "Officer", "Manager", "Admin", "Compliance", "Auditor" };
        foreach (var role in roles)
        {
            statistics[$"Role_{role}"] = allUsers.Count(u => u.Role == role);
        }

        // Count by status (you'd need to implement status checking)
        statistics["Total_Users"] = allUsers.Count();
        statistics["Recent_Logins"] = allUsers.Count(u => u.LastLogin.HasValue && u.LastLogin.Value > DateTime.UtcNow.AddDays(-7));

        return statistics;
    }
}
