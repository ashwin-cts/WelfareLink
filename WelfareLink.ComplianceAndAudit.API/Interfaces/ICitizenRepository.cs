using WelfareLink.ComplianceAndAudit.API.Models;

namespace WelfareLink.ComplianceAndAudit.API.Interfaces;

public interface ICitizenRepository
{
    Task<Citizen> GetByIdAsync(int id);
    Task<Citizen> GetByUserIdAsync(int userId); // Changed to int
    Task AddAsync(Citizen citizen);
    Task UpdateAsync(Citizen citizen);


}
