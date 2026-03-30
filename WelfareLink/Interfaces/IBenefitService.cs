using WelfareLink.Models;

namespace WelfareLink.Interfaces
{
    public interface IBenefitService
    {
        Task<IEnumerable<Benefit>> GetAllBenefitsAsync();
        Task<Benefit?> GetBenefitByIdAsync(int id);
        Task<Benefit> CreateBenefitAsync(Benefit benefit, int officerId = 0);
        Task<Benefit> UpdateBenefitAsync(Benefit benefit, int officerId = 0);
        Task<bool> DeleteBenefitAsync(int id);
        Task<bool> BenefitExistsAsync(int id);
        Task<Benefit?> CreateBenefitForApprovedApplicationAsync(int applicationId);
    }
}
