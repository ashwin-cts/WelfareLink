export interface WelfareApplication {
  applicationID: number; 
  citizenID: number;
  programID: number;
  status: 'Pending' | 'Rejected' | 'Approved' | 'Fully Disbursed' | 'Under Review';
  submittedDate: string; 

  citizen?: {
    name:string;
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
}

