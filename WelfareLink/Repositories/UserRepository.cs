using Microsoft.EntityFrameworkCore;
using WelfareLink.Interfaces;
using WelfareLink.Models;
using WelfareLink.Data; 

namespace WelfareLink.Repositories { 

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(WelfareLinkDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<IEnumerable<User>> GetByRoleAsync(string role)
    {
        return await _dbSet
            .Where(u => u.Role == role)
            .OrderBy(u => u.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetByStatusAsync(string status)
    {
        return await _dbSet
            .Where(u => EF.Property<string>(u, "Status") == status)
            .OrderBy(u => u.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm)
    {
        return await _dbSet
            .Where(u => u.Name.Contains(searchTerm) ||
                       u.Email.Contains(searchTerm) ||
                       u.Phone.Contains(searchTerm))
            .OrderBy(u => u.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetUsersByRoleAndStatusAsync(string role, string status)
    {
        return await _dbSet
            .Where(u => u.Role == role && EF.Property<string>(u, "Status") == status)
            .OrderBy(u => u.Name)
            .ToListAsync();
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbSet
            .AnyAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<bool> UpdateLastLoginAsync(string userId)
    {
        var user = await GetByIdAsync(userId);
        if (user == null)
            return false;

        user.LastLogin = DateTime.UtcNow;
        return true;
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        return await _dbSet
            .Where(u => EF.Property<string>(u, "Status") == "Active")
            .OrderBy(u => u.Name)
            .ToListAsync();
    }

    public async Task<int> GetUserCountByRoleAsync(string role)
    {
        return await _dbSet
            .CountAsync(u => u.Role == role);
    }

    public async Task<IEnumerable<User>> GetRecentlyCreatedUsersAsync(int days)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        return await _dbSet
            .Where(u => EF.Property<DateTime>(u, "CreatedDate") >= cutoffDate)
            .OrderByDescending(u => EF.Property<DateTime>(u, "CreatedDate"))
            .ToListAsync();
    }
}


    }