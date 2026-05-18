using WelfareLink.ComplianceAndAuditLog.API.Models;

namespace WelfareLink.ComplianceAndAuditLog.API.Interfaces;

public interface IResourceRepository
{
    Task<IEnumerable<Resource>> GetAllResourcesAsync();
    Task<IEnumerable<Resource>> GetResourcesByProgramIdAsync(int programId);
    Task AddResourcesAsync(Resource resource);
    Task UpdateResourceAsync(Resource resource);
}
