using WelfareLink.Models;

namespace WelfareLink.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalAudits { get; set; }
        public int PendingAudits { get; set; }
        public int CompletedAudits { get; set; }
        public int TotalComplainceRecords { get; set; }
        public int TotalUsers { get; set; }
        
        public List<Audit> RecentAudits { get; set; } = new List<Audit>();
        public List<ComplainceRecord> RecentComplainceRecords { get; set; } = new List<ComplainceRecord>();
        
        public Dictionary<string, int> AuditsByStatus { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> ComplianceByType { get; set; } = new Dictionary<string, int>();
    }
}
