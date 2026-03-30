using WelfareLink.Interfaces;
using WelfareLink.Models;
using WelfareLink.Data;
using Microsoft.EntityFrameworkCore;

namespace WelfareLink.Repositories;

public class UserRepository : IUserRepository
{
    private readonly WelfareLinkDbContext _context;

    public UserRepository(WelfareLinkDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users
            .OrderBy(u => u.Name)
            .ToListAsync();
    }

    public async Task<User> GetByIdAsync(string userId)
    {
        return await _context.Users.FindAsync(userId);
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}
