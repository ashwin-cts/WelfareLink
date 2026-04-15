using WelfareLink.Models;

namespace WelfareLink.Models
{
    public class ApplicationDetailsViewModel
    {
        public WelfareApplication Application { get; set; } = null!;
        public ProgramResourcesDto? ProgramResources { get; set; }
    }
}
