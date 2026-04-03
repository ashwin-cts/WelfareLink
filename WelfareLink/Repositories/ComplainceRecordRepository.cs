using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WelfareLink.Data;
using WelfareLink.Interfaces;
using WelfareLink.Models;

namespace WelfareLink.Repositories;

public class ComplainceRecordRepository : IComplainceRecordRepository
{
    private readonly WelfareLinkDbContext _db;

    public ComplainceRecordRepository(WelfareLinkDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<ComplainceRecord>> GetAllAsync()
    {
        return await _db.ComplainceRecords.ToListAsync();
    }

    public async Task<ComplainceRecord?> GetByIdAsync(string id)
    {
        return await _db.ComplainceRecords.FindAsync(id);
    }

    public async Task AddAsync(ComplainceRecord record)
    {
        await _db.ComplainceRecords.AddAsync(record);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(ComplainceRecord record)
    {
        _db.ComplainceRecords.Update(record);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id)
    {
        var record = await GetByIdAsync(id);
        if (record is not null)
        {
            _db.ComplainceRecords.Remove(record);
            await _db.SaveChangesAsync();
        }
    }
}
