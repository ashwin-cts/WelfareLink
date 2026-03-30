using WelfareLink.Interfaces;
using WelfareLink.Models;

namespace WelfareLink.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repo;

    public UserService(IUserRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _repo.GetAllAsync();
    }

    public async Task<User> GetUserByIdAsync(string userId)
    {
        return await _repo.GetByIdAsync(userId);
    }

    public async Task<bool> CreateUserAsync(User user)
    {
        if (string.IsNullOrEmpty(user.UserID))
            user.UserID = Guid.NewGuid().ToString();

        await _repo.AddAsync(user);
        return true;
    }

    public async Task<bool> UpdateUserAsync(User user)
    {
        await _repo.UpdateAsync(user);
        return true;
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        await _repo.DeleteAsync(userId);
        return true;
    }
}
