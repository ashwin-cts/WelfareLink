using WelfareLink.Interfaces;
using WelfareLink.Models;

namespace WelfareLink.Services;

public class ResourceService : IResourceService
{
    private readonly IResourceRepository _resourceRepository;
    private readonly IWelfareProgramRepository _programRepository;

    public ResourceService(IResourceRepository resourceRepository, IWelfareProgramRepository programRepository)
    {
        _resourceRepository = resourceRepository;
        _programRepository = programRepository;
    }

    public async Task<IEnumerable<Resource>> GetAllResourcesAsync()
    {
        return await _resourceRepository.GetAllResourcesAsync();
    }

    public async Task<IEnumerable<Resource>> GetResourcesByProgramIdAsync(int programId)
    {
        return await _resourceRepository.GetResourcesByProgramIdAsync(programId);
    }

    public async Task AddResourceAsync(Resource resource)
    {
        await ValidateProgramExists(resource.ProgramID);
        ValidateResourceQuantity(resource);
        await ValidateResourceAgainstBudget(resource, excludeResourceId: null); // No exclusion for new resources

        resource.Status = "Available";

        await _resourceRepository.AddResourcesAsync(resource);
    }

    public async Task UpdateResourceAsync(Resource resource)
    {
        await ValidateProgramExists(resource.ProgramID);
        ValidateResourceQuantity(resource);
        await ValidateResourceAgainstBudget(resource, excludeResourceId: resource.ResourceID); // Exclude current resource from calculation

        await _resourceRepository.UpdateResourceAsync(resource);
    }

    private async Task ValidateProgramExists(int programId)
    {
        var program = await _programRepository.GetProgramByIdAsync(programId);
        if (program == null)
        {
            throw new InvalidOperationException($"Program with ID {programId} not found.");
        }

        if (program.Status != "Active")
        {
            throw new InvalidOperationException("Cannot allocate resources to a non-active programme.");
        }
    }

    private void ValidateResourceQuantity(Resource resource)
    {
        if (resource.Quantity <= 0)
        {
            throw new InvalidOperationException("Resource quantity must be greater than zero.");
        }
    }

    private async Task ValidateResourceAgainstBudget(Resource resource, int? excludeResourceId)
    {
        var program = await _programRepository.GetProgramByIdAsync(resource.ProgramID);
        if (program == null)
        {
            throw new InvalidOperationException("Programme not found.");
        }

        var existingResources = await _resourceRepository.GetResourcesByProgramIdAsync(resource.ProgramID);

        // Only validate budget for "Funds" type resources
        if (resource.Type?.Equals("Funds", StringComparison.OrdinalIgnoreCase) == true)
        {
            // Calculate total allocated funds, excluding the current resource if updating
            var totalAllocatedFunds = existingResources
                .Where(r => r.Type != null &&
                            r.Type.Equals("Funds", StringComparison.OrdinalIgnoreCase) &&
                            (!excludeResourceId.HasValue || r.ResourceID != excludeResourceId.Value)) // Exclude current resource for updates
                .Sum(r => r.Quantity);

            var remainingBudget = program.Budget - totalAllocatedFunds;
            var newTotalFunds = totalAllocatedFunds + resource.Quantity;

            if (newTotalFunds > program.Budget)
            {
                var excessAmount = newTotalFunds - program.Budget;
                throw new InvalidOperationException(
                    $"Cannot allocate ₹{resource.Quantity:N2}. Programme budget: ₹{program.Budget:N2}, Already allocated: ₹{totalAllocatedFunds:N2}, Remaining: ₹{remainingBudget:N2}. Exceeds by ₹{excessAmount:N2}. Please increase programme budget or reduce allocation amount.");
            }
        }
    }
}
