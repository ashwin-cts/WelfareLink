using System.Collections.Generic;
using System.Threading.Tasks;
using WelfareLink.Models;

namespace WelfareLink.Interfaces;

public interface IComplainceRecordRepository
{
    Task<IEnumerable<ComplainceRecord>> GetAllAsync();

    Task<ComplainceRecord?> GetByIdAsync(string id);

    Task AddAsync(ComplainceRecord record);

    Task UpdateAsync(ComplainceRecord record);

    Task DeleteAsync(string id);
}
