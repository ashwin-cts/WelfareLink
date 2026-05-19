export interface WelfareApplication {
  applicationID: number;
  citizenID: number;
  programID: number;
  status: 'Pending' | 'Rejected' | 'Approved' | 'Fully Disbursed' | 'Under Review';
  submittedDate: string;

  citizen?: {
    name: string;
    email?: string;
    registrationDate?: string;
  };

  program?: {
    title: string;
  };

  eligibilityChecks?: EligibilityCheck[];
  applicationDocuments?: any[];
}

export interface EligibilityCheck {
  checkID: number;
  applicationID: number; // Make sure this matches the exact casing expected by your backend
  officerID: number;
  result: string;
  resultCode: string;
  date: string;
  notes: string;
}

export interface DashboardStats {
  total: number;
  pending: number;
  approved: number;
  rejected: number;
  fullyDisbursed: number;
}
// ----------Welfare Application Analytics----------------
export interface AppAnalyticsDashboard {
  TotalApplications: number;
  PendingApplications: number;
  ApprovedApplications: number;
  RejectedApplications: number;
  UnderReviewApplications: number;
  ApprovalRate: number;
  TotalChecks: number;
  EligibleChecks: number;
  IneligibleChecks: number;
  ApplicationsByMonth: { month: string; count: number }[];
}

export interface AppStatusBreakdown {
  status: string;
  count: number;
  percentage: number;
}

// Wrapper for the Monthly Trends response
export interface AppMonthlyTrendResponse {
  Year: number;
  MonthlyData: AppMonthlyTrendData[];
}

export interface AppMonthlyTrendData {
  month: string;
  total: number;
  pending: number;
  approved: number;
  rejected: number;
  underReview: number;
}

// Wrapper for the Eligibility Report response
export interface AppEligibilityReport {
  ResultBreakdown: { result: string; count: number; percentage: number }[];
  ChecksByMonth: AppEligibilityCheckMonth[];
  TotalApplicationsChecked: number;
}

export interface AppEligibilityCheckMonth {
  month: string;
  total: number;
  eligible: number;
  ineligible: number;
}

export interface AppApprovalRate {
  approvalRate: number;
}
export interface ComplianceRecord {
  recordID: number;
  entityType: string;
  entityId: number;
  violationType: string;
  description: string;
  status: string;
  notes?: string;
}
