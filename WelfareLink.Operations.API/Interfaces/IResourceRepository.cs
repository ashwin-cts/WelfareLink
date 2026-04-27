using WelfareLink.Operations.API.Models;

namespace WelfareLink.Operations.API.Interfaces;

public interface IResourceRepository
{
    Task<IEnumerable<Resource>> GetAllResourcesAsync();
    Task<IEnumerable<Resource>> GetResourcesByProgramIdAsync(int programId);
    Task AddResourcesAsync(Resource resource);
    Task UpdateResourceAsync(Resource resource);
}
