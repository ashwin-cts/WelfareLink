using WelfareLink.Models;
namespace WelfareLink.Interfaces
{
public interface IUserRepository: IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetByRoleAsync(string role);
    Task<IEnumerable<User>> GetByStatusAsync(string status);
    Task<IEnumerable<User>> SearchUsersAsync(string searchTerm);
    Task<IEnumerable<User>> GetUsersByRoleAndStatusAsync(string role, string status);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> UpdateLastLoginAsync(string userId);
    Task<IEnumerable<User>> GetActiveUsersAsync();
    Task<int> GetUserCountByRoleAsync(string role);
    Task<IEnumerable<User>> GetRecentlyCreatedUsersAsync(int days);
}
}
