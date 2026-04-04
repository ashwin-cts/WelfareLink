using WelfareLink.Models;

namespace WelfareLink.Interfaces
{
    public interface IDisbursementRepository
    {
        Task<IEnumerable<Disbursement>> GetAllAsync();
        Task<Disbursement?> GetByIdAsync(int id);
        Task<Disbursement> AddAsync(Disbursement disbursement);
        Task<Disbursement> UpdateAsync(Disbursement disbursement);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<Disbursement>> GetByBenefitIdAsync(int benefitId);
        Task<double> GetCompletedDisbursementTotalForProgramAsync(int programId, int excludeDisbursementId);
    }
}