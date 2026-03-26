namespace WelfareLink.ViewModels
{
    public class AnalyticsDashboardViewModel
    {
        // Summary Metrics
        public int TotalAllocated { get; set; }
        public int TotalDisbursed { get; set; }
        public int TotalPending { get; set; }
        public int TotalFailed { get; set; }

        // Financial Metrics
        public double TotalAmountAllocated { get; set; }
        public double TotalAmountDisbursed { get; set; }

        // Efficiency Metrics
        public double DisbursementEfficiency { get; set; }
        public double AllocationRate { get; set; }

        // Benefit Type Breakdown
        public List<BenefitTypeBreakdown> BenefitTypeBreakdowns { get; set; } = new();

        // Recent Activity
        public List<RecentDisbursement> RecentDisbursements { get; set; } = new();

        // Monthly Trends
        public List<MonthlyTrend> MonthlyTrends { get; set; } = new();
    }

    public class BenefitTypeBreakdown
    {
        public string Type { get; set; } = string.Empty;
        public int Count { get; set; }
        public double TotalAmount { get; set; }
        public double DisbursedAmount { get; set; }
        public int DisbursedCount { get; set; }
        public double Percentage { get; set; }
    }

    public class RecentDisbursement
    {
        public int DisbursementID { get; set; }
        public string BenefitType { get; set; } = string.Empty;
        public int CitizenID { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class MonthlyTrend
    {
        public string Month { get; set; } = string.Empty;
        public int Allocated { get; set; }
        public int Disbursed { get; set; }
        public double AllocatedAmount { get; set; }
        public double DisbursedAmount { get; set; }
    }
}
