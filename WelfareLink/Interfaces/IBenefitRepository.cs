using WelfareLink.Models;
namespace WelfareLink.Interfaces
{
    public interface IBenefitRepository
    {
        Task<IEnumerable<Benefit>> GetAllAsync();
        Task<Benefit?> GetByIdAsync(int id);
        Task<Benefit> AddAsync(Benefit benefit);
        Task<Benefit> UpdateAsync(Benefit benefit);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
