using WelfareLink.Interfaces;
using WelfareLink.Models; // Change this to your project's Models namespace if different

namespace WelfareLink.Services
{
    public class ComplainceRecordService : IComplainceRecordService
    {
        private readonly IComplainceRecordRepository _repository;

        public ComplainceRecordService(IComplainceRecordRepository repository)
        {
            _repository = repository;
        }

        // GET: All compliance records
        public async Task<IEnumerable<ComplianceRecord>> GetAllRecordsAsync()
        {
            return await _repository.GetAllAsync();
        }

        // GET: Single compliance record by ID
        public async Task<ComplianceRecord> GetRecordByIdAsync(string complianceId)
        {
            if (string.IsNullOrWhiteSpace(complianceId)) return null;

            return await _repository.GetByIdAsync(complianceId);
        }

        // CREATE: Apply business logic and save to DB
        public async Task<bool> CreateRecordAsync(ComplianceRecord record)
        {
            if (record == null) return false;

            // Business Logic: Auto-generate GUID if not provided by UI
            if (string.IsNullOrWhiteSpace(record.ComplianceID))
            {
                record.ComplianceID = Guid.NewGuid().ToString();
            }

            // Set standard server time (Standardizes dates across users)
            record.Date = DateTime.UtcNow;

            await _repository.AddAsync(record);
            return true;
        }

        // GET: Filter records based on an Entity ID
        public async Task<IEnumerable<ComplianceRecord>> GetRecordsByEntityAsync(string entityId)
        {
            if (string.IsNullOrWhiteSpace(entityId)) return Enumerable.Empty<ComplianceRecord>();

            var allRecords = await _repository.GetAllAsync();
            
            // Return filtered records where entity matches
            return allRecords.Where(r => r.EntityID == entityId);
        }
    }
}
