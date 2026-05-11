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
  applicationId: number; 
  officerID: number;
  date: string;
  result: string;
  resultCode: string;
  notes?: string;
}

export interface DashboardStats {
  total: number;
  pending: number;
  approved: number;
  rejected: number;
}

