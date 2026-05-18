using WelfareLink.Services;

namespace WelfareLink.Models
{
    public class ComplainceRecordDetailsViewModel
    {
        public ComplianceRecord ComplianceRecord { get; set; } = null!;
        public WelfareApplication? Application { get; set; }
        public ProgramResourcesDto? ProgramResources { get; set; }
    }
}
