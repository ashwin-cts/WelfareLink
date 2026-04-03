using System.Collections.Generic;
using System.Threading.Tasks;
using WelfareLink.Interfaces;
using WelfareLink.Models;

namespace WelfareLink.Services;

public class ComplainceRecordService : IComplainceRecordService
{
    private readonly IComplainceRecordRepository _repo;

    public ComplainceRecordService(IComplainceRecordRepository repo)
    {
        _repo = repo;
    }

    public Task<IEnumerable<ComplainceRecord>> GetAllAsync() => _repo.GetAllAsync();

    public Task<ComplainceRecord?> GetByIdAsync(string id) => _repo.GetByIdAsync(id);

    public Task CreateAsync(ComplainceRecord record) => _repo.AddAsync(record);

    public Task UpdateAsync(ComplainceRecord record) => _repo.UpdateAsync(record);

    public Task DeleteAsync(string id) => _repo.DeleteAsync(id);
}
