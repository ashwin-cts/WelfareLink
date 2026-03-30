using Microsoft.EntityFrameworkCore;
using WelfareLink.Data; // Ensure this points to your DbContext folder
using WelfareLink.Interfaces;
using WelfareLink.Models; // Assuming ComplianceRecord is inside the Models folder

namespace WelfareLink.Repositories
{
    public class ComplainceRecordRepository : IComplainceRecordRepository
    {
        private readonly ApplicationDbContext _context;

        public ComplainceRecordRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: All compliance records, ordered by newest date
        public async Task<IEnumerable<ComplianceRecord>> GetAllAsync()
        {
            return await _context.ComplianceRecords
                .OrderByDescending(c => c.Date)
                .ToListAsync();
        }

        // GET: A single compliance record by ID
        public async Task<ComplianceRecord> GetByIdAsync(string complianceId)
        {
            if (string.IsNullOrWhiteSpace(complianceId)) return null;

            return await _context.ComplianceRecords.FindAsync(complianceId);
        }

        // CREATE: Add a new record to the DB context
        public async Task AddAsync(ComplianceRecord record)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));

            await _context.ComplianceRecords.AddAsync(record);
            await _context.SaveChangesAsync();
        }

        // UPDATE: Modify an existing record in the DB context
        public async Task UpdateAsync(ComplianceRecord record)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));

            _context.ComplianceRecords.Update(record);
            await _context.SaveChangesAsync();
        }
    }
}
