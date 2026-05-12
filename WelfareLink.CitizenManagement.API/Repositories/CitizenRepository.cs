using Microsoft.EntityFrameworkCore;    
using WelfareLink.CitizenManagement.API.Data;
using WelfareLink.CitizenManagement.API.Interfaces;
using WelfareLink.CitizenManagement.API.Models;
using WelfareLink.CitizenManagement.API.DTOs;

namespace WelfareLink.CitizenManagement.API.Repositories;

public class CitizenRepository : ICitizenRepository
{
    private readonly WelfareLinkDbContext _context;

    public CitizenRepository(WelfareLinkDbContext context)
    {
        _context = context;
    }

    public async Task<Citizen> GetByIdAsync(int id)
    {
        return await _context.Citizens
            .AsNoTracking()
            .Include(c => c.User) // <--- THIS IS REQUIRED TO GET THE EMAIL
            .Include(c => c.CitizenDocuments)
            .FirstOrDefaultAsync(c => c.CitizenId == id);
    }

    public async Task<Citizen> GetByUserIdAsync(int userId)
    {
        return await _context.Citizens
            .AsNoTracking()
            .Include(c => c.User) // <--- THIS IS REQUIRED TO GET THE EMAIL
            .Include(c => c.CitizenDocuments)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task AddAsync(Citizen citizen)
    {
        await _context.Citizens.AddAsync(citizen);
        await _context.SaveChangesAsync();
    }

    // Make sure you update your ICitizenRepository interface to accept UpdateCitizenDto!
    public async Task UpdateAsync(UpdateCitizenDto dto)
    {
        // 1. We MUST .Include() the User table so we have access to existing.User
        var existing = await _context.Citizens
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.CitizenId == dto.CitizenId);

        if (existing == null)
            throw new InvalidOperationException($"Citizen with ID {dto.CitizenId} not found.");

        // 2. Safely map the Citizen fields
        existing.Name = dto.Name;
        existing.ContactInfo = dto.ContactInfo;
        existing.Address = dto.Address;

        // 3. Safely map the User fields (Email AND FullName!)
        if (existing.User != null)
        {
            existing.User.Email = dto.Email;
            existing.User.FullName = dto.Name; // <--- ADD THIS EXACT LINE
        }

        // 4. Save everything to both tables at once!
        await _context.SaveChangesAsync();
    }
}

