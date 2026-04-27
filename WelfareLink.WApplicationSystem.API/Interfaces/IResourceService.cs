using WelfareLink.WApplicationSystem.API.Models;

namespace WelfareLink.WApplicationSystem.API.Interfaces;

public interface IResourceService
{
    Task<IEnumerable<Resource>> GetAllResourcesAsync();
    Task<IEnumerable<Resource>> GetResourcesByProgramIdAsync(int programId);
    Task AddResourceAsync(Resource resource);
    Task UpdateResourceAsync(Resource resource);
}
