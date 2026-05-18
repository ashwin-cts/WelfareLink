using WelfareLink.ProgramManagement.API.Interfaces;
using WelfareLink.ProgramManagement.API.Models;
using WelfareLink.ProgramManagement.API.Data;
using Microsoft.EntityFrameworkCore;    

namespace WelfareLink.ProgramManagement.API.Repositories;

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
            .Include(c => c.CitizenDocuments)
            .FirstOrDefaultAsync(c => c.CitizenId == id);
    }

    public async Task<Citizen> GetByUserIdAsync(int userId)
    {
        return await _context.Citizens
            .AsNoTracking()
            .Include(c => c.CitizenDocuments)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task AddAsync(Citizen citizen)
    {
        await _context.Citizens.AddAsync(citizen);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Citizen citizen)
    {
        var existing = await _context.Citizens.FindAsync(citizen.CitizenId);
        if (existing == null)
            throw new InvalidOperationException($"Citizen with ID {citizen.CitizenId} not found.");
        _context.Entry(existing).CurrentValues.SetValues(citizen);
        await _context.SaveChangesAsync();
    }
}

