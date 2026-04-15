using WelfareLinkApi.Interfaces;
using WelfareLinkApi.Models;

namespace WelfareLinkApi.Services;

public class ComplainceRecordService : IComplainceRecordService
{
    private readonly IComplainceRecordRepository _repo;

    public ComplainceRecordService(IComplainceRecordRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<ComplainceRecord>> GetAllRecordsAsync()
        => await _repo.GetAllAsync();

    public async Task<ComplainceRecord?> GetRecordByIdAsync(int id)
        => await _repo.GetByIdAsync(id);

    public async Task<IEnumerable<ComplainceRecord>> GetOpenRecordsAsync()
        => await _repo.GetByStatusAsync("Open");

    public async Task<ComplainceRecord> CreateRecordAsync(ComplainceRecord record)
    {
        record.CreatedDate = DateTime.UtcNow;
        record.Status = "Open";
        return await _repo.AddAsync(record);
    }

    public async Task<ComplainceRecord> UpdateStatusAsync(int id, string status, int? resolvedByUserId, string? notes)
    {
        var record = await _repo.GetByIdAsync(id)
            ?? throw new InvalidOperationException($"Compliance record #{id} not found.");

        record.Status = status;
        if (notes != null) record.Notes = notes;
        if (status is "Resolved" or "Dismissed")
        {
            record.ResolvedDate = DateTime.UtcNow;
            record.ResolvedByUserId = resolvedByUserId;
        }
        await _repo.UpdateAsync(record);
        return record;
    }
}
