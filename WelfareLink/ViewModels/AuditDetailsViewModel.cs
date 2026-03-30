<<<<<<< HEAD
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
=======
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
>>>>>>> 67010b637ee5fae89ead73a246ac714beea4c426
