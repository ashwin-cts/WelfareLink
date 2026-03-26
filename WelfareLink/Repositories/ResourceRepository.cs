using WelfareLink.Interfaces;
using WelfareLink.Models;
using WelfareLink.Data;
using Microsoft.EntityFrameworkCore;
namespace WelfareLink.Repositories;

public class ResourceRepository : IResourceRepository
{
    private readonly WelfareLinkDbContext _context;
    public ResourceRepository(WelfareLinkDbContext context)
    {
        _context = context;
    }
    public async Task<IEnumerable<Resource>> GetAllResourcesAsync()
    {
        return await _context.Resources.Include(r => r.Program).ToListAsync();
    }
    public async Task<IEnumerable<Resource>> GetResourcesByProgramIdAsync(int programId)
    {
        return await _context.Resources.Where(r => r.ProgramID == programId).ToListAsync();
    }
    public async Task AddResourcesAsync(Resource resource)
    {
        await _context.Resources.AddAsync(resource);
        await _context.SaveChangesAsync();
    }
    public async Task UpdateResourceAsync(Resource resource)
    {
        // Load existing entity to avoid tracking conflicts
        var existingResource = await _context.Resources.FindAsync(resource.ResourceID);
        if (existingResource != null)
        {
            // Update only the properties that should be modified
            existingResource.ProgramID = resource.ProgramID;
            existingResource.Type = resource.Type;
            existingResource.Quantity = resource.Quantity;
            existingResource.Status = resource.Status;

            await _context.SaveChangesAsync();
        }
    }
}
