using System.Collections.Generic;
using WelfareLink.Models;

namespace WelfareLink.ViewModels
{
    public class DashboardViewModel
    {
        public IEnumerable<Audit> Audits { get; set; } = new List<Audit>();
        public IEnumerable<ComplainceRecord> ComplainceRecords { get; set; } = new List<ComplainceRecord>();
    }
}
