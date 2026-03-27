using WelfareLink.Interfaces;
using WelfareSystem.Models; // Change this to your project's Models namespace if different

namespace WelfareLink.Services
{
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
    }
}
