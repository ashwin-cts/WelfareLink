using Microsoft.EntityFrameworkCore;
using WelfareLink.ProgramManagement.API.Data;
using WelfareLink.ProgramManagement.API.Interfaces;
using WelfareLink.ProgramManagement.API.Models;

namespace WelfareLink.ProgramManagement.API.Repositories;

public class UserRepository : IUserRepository
{
    private readonly WelfareLinkDbContext _context;

    public UserRepository(WelfareLinkDbContext context)
    {
        _context = context;
    }

    public async Task<User> AddAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetByUsernameAndRoleAsync(string username, string role)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Username == username && u.Role == role);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<bool> UpdateAsync(User user)
    {
        _context.Users.Update(user);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await GetByIdAsync(id);
        if (user == null) return false;

        _context.Users.Remove(user);
        return await _context.SaveChangesAsync() > 0;
    }
}
