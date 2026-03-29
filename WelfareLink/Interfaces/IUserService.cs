using WelfareLink.Models;

namespace WelfareLink.Interfaces;

public interface IUserService
{
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(string userId);
    Task<User?> GetUserByEmailAsync(string email);
    Task<IEnumerable<User>> GetUsersByRoleAsync(string role);
    Task<IEnumerable<User>> SearchUsersAsync(string searchTerm, string? roleFilter = null, string? statusFilter = null);
    Task<User> CreateUserAsync(User user, string temporaryPassword);
    Task<User> UpdateUserAsync(User user);
    Task<bool> DeleteUserAsync(string userId);
    Task<bool> SuspendUserAsync(string userId, string reason);
    Task<bool> ActivateUserAsync(string userId);
    Task<bool> DeactivateUserAsync(string userId);
    Task<bool> EmailExistsAsync(string email);
    Task<int> GetUserCountByRoleAsync(string role);
    Task<IEnumerable<User>> GetRecentlyCreatedUsersAsync(int days);
    Task<bool> UpdateUserProfileAsync(string userId, string name, string email, string? phone);
    Task<Dictionary<string, int>> GetUserStatisticsAsync();
}
