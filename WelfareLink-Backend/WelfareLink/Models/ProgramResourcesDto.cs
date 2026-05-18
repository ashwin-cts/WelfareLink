using System.Collections.Generic;

namespace WelfareLink.Models
{
    public class ProgramResourcesDto
    {
        public string ProgramTitle { get; set; }
        public decimal ProgramBudget { get; set; }
        public decimal TotalAllocated { get; set; }
        public decimal RemainingBudget { get; set; }
        public List<ResourceDto> Resources { get; set; }
    }

    public class ResourceDto
    {
        public int ResourceID { get; set; }
        public string Name { get; set; }
        public decimal AmountAllocated { get; set; }
    }
}
