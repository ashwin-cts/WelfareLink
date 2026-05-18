using WelfareLink.UserManagement.API.Models;

namespace WelfareLink.UserManagement.API.ViewModels
{
    public class ProgramDetailViewModel
    {
        public WelfareProgram Program { get; set; }
        public IEnumerable<Resource> Resources { get; set; }
        public int ApplicationCount { get; set; }
        public decimal TotalAllocatedFunds { get; set; }
        public decimal TotalAllocatedMaterials { get; set; }
        public decimal UtilisationPercentage { get; set; }
        public decimal RemainingBudget { get; set; }
        public bool IsBudgetCritical { get; set; }
    }
}
