using WelfareLink.ComplianceAndAudit.API.Models;

namespace WelfareLink.ComplianceAndAudit.API.Interfaces;

public interface IResourceService
{
    Task<IEnumerable<Resource>> GetAllResourcesAsync();
    Task<IEnumerable<Resource>> GetResourcesByProgramIdAsync(int programId);
    Task AddResourceAsync(Resource resource);
    Task UpdateResourceAsync(Resource resource);
}
