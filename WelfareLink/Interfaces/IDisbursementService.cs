using WelfareLink.Models;

namespace WelfareLink.Interfaces
{
    public interface IDisbursementService
    {
        Task<IEnumerable<Disbursement>> GetAllDisbursementsAsync();
        Task<Disbursement?> GetDisbursementByIdAsync(int id);
        Task<Disbursement> CreateDisbursementAsync(Disbursement disbursement);
        Task<Disbursement> UpdateDisbursementAsync(Disbursement disbursement);
        Task<bool> DeleteDisbursementAsync(int id);
        Task<bool> DisbursementExistsAsync(int id);
        Task<IEnumerable<Disbursement>> GetDisbursementsByBenefitIdAsync(int benefitId);
    }
}