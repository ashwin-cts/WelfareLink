namespace WelfareLink.ViewModels
{
    public class ProgramPerformanceViewModel
    {
        public int ProgramID { get; set; }
        public string ProgramTitle { get; set; }
        public int TotalApplications { get; set; }
        public int ApprovedApplications { get; set; }
        public int RejectedApplications { get; set; }
        public int PendingApplications { get; set; }
        public decimal ApprovalRate { get; set; }
        public int BenefitsDisbursed { get; set; }
        public int CitizenCount { get; set; }
        public decimal BudgetUtilisation { get; set; }
        public string Status { get; set; }
    }
}
