using WelfareLink.Models;

namespace WelfareLink.ViewModels
{
    public class AuditDetailsViewModel
    {
        public Audit Audit { get; set; }
        public User Officer { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public List<string> AvailableStatuses { get; set; } = new List<string>
        {
            "Pending",
            "In Progress",
            "Completed",
            "Cancelled"
        };
    }
}
