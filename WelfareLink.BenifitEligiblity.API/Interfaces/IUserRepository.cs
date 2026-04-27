using WelfareLink.BenifitEligiblity.API.Models;

namespace WelfareLink.BenifitEligiblity.API.Interfaces;

public interface IUserRepository
{
    Task<User> AddAsync(User user);
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByUsernameAndRoleAsync(string username, string role);
    Task<IEnumerable<User>> GetAllAsync();
    Task<bool> UpdateAsync(User user);
    Task<bool> DeleteAsync(int id);
}
