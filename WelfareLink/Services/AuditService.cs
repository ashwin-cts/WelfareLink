using WelfareLink.Interfaces;
<<<<<<< HEAD
using WelfareLink.Repositories; 
using WelfareLink.Data;

using WelfareLink.Models;
=======
using WelfareSystem.Models; // Change this to your project's Models namespace if different
>>>>>>> 67010b637ee5fae89ead73a246ac714beea4c426

namespace WelfareLink.Services
{
<<<<<<< HEAD
    private readonly IAuditRepository _repo;

    public AuditService(IAuditRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<Audit>> GetAllAuditsAsync()
    {
        return await _repo.GetAllAsync();
    }

    public async Task<Audit> GetAuditByIdAsync(string auditId)
    {
        return await _repo.GetByIdAsync(auditId);
    }

    public async Task<bool> CreateAuditAsync(Audit audit)
    {
        if (string.IsNullOrEmpty(audit.AuditID))
            audit.AuditID = Guid.NewGuid().ToString();

        audit.Date = DateTime.UtcNow;

        await _repo.AddAsync(audit);
        return true;
    }

    public async Task<bool> UpdateAuditStatusAsync(string auditId, string status, string findings)
    {
        var audit = await _repo.GetByIdAsync(auditId);
        if (audit == null)
            return false;

        audit.Status = status;
        audit.Findings = findings;

        await _repo.UpdateAsync(audit);
        return true;
=======
    public class AuditService : IAuditService
    {
        private readonly IAuditRepository _auditRepository;

        public AuditService(IAuditRepository auditRepository)
        {
            _auditRepository = auditRepository;
        }

        // GET: All audits
        public async Task<IEnumerable<Audit>> GetAllAuditsAsync()
        {
            return await _auditRepository.GetAllAsync();
        }

        // GET: Single audit by ID
        public async Task<Audit> GetAuditByIdAsync(string auditId)
        {
            if (string.IsNullOrWhiteSpace(auditId)) return null;

            return await _auditRepository.GetByIdAsync(auditId);
        }

        // CREATE: Business rules for creating an Audit
        public async Task<bool> CreateAuditAsync(Audit audit)
        {
            if (audit == null) return false;

            // Auto-generate ID if not provided by the UI
            if (string.IsNullOrWhiteSpace(audit.AuditID))
            {
                audit.AuditID = Guid.NewGuid().ToString();
            }

            // Set current time for the server audit
            audit.Date = DateTime.UtcNow;

            await _auditRepository.AddAsync(audit);
            return true;
        }

        // UPDATE: Business rules for modifying status and findings
        public async Task<bool> UpdateAuditStatusAsync(string auditId, string status, string findings)
        {
            if (string.IsNullOrWhiteSpace(auditId)) return false;

            var existingAudit = await _auditRepository.GetByIdAsync(auditId);
            if (existingAudit == null) return false;

            // Apply updates
            existingAudit.Status = status;
            existingAudit.Findings = findings;

            await _auditRepository.UpdateAsync(existingAudit);
            return true;
        }
>>>>>>> 67010b637ee5fae89ead73a246ac714beea4c426
    }
}
