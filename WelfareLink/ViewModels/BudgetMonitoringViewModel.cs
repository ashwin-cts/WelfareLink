namespace WelfareLink.ViewModels
{
    public class BudgetMonitoringViewModel
    {
        public int ProgramID { get; set; }
        public string ProgramTitle { get; set; }
        public decimal TotalBudget { get; set; }
        public decimal AllocatedFunds { get; set; }
        public decimal DisbursedFunds { get; set; }
        public decimal RemainingBudget { get; set; }
        public decimal UtilisationPercentage { get; set; }
        public string Status { get; set; }
        public bool IsCritical { get; set; }
    }

    public class BudgetDashboardViewModel
    {
        public IEnumerable<BudgetMonitoringViewModel> ProgramBudgets { get; set; }
        public decimal TotalBudgetAllPrograms { get; set; }
        public decimal TotalAllocated { get; set; }
        public decimal TotalRemaining { get; set; }
        public int CriticalProgramsCount { get; set; }
    }
}
