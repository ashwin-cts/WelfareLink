export interface ComplianceMetrics {
  total: number;
  open: number;
  resolved: number;
  issuesByType?: Array<{ violationType: string; count: number }>;
}

export interface DashboardApplication {
  applicationID: number;
  citizenName: string;
  citizenID: number;
  programTitle: string;
  applicationStatus: string;
  maxBenefit?: number;
  totalBenefitAllocated?: number;
  totalDisbursed?: number;
  isFlagged?: boolean;
}

export interface CitizenDocumentDto {
  documentID: number; // <-- Make sure this is added
  docType: string;
  documentName: string;
  uploadedDate: string;
}
// ... rest of your interfaces

export interface ApplicationDocumentLink {
  citizenDocument?: CitizenDocumentDto;
}

export interface DisbursementDetail {
  disbursementID: number;
  amount: number;
  date: string;
  status: string;
}

export interface BenefitDetail {
  benefitID: number;
  type: string;
  amount: number;
  status: string;
  date: string;
  disbursements?: DisbursementDetail[];
}

export interface ApplicationDetail {
  applicationID: number;
  citizenID: number;
  citizen?: { name: string; };
  program?: { title: string; maxBenefitPerCitizen?: number; };
  submittedDate?: string;
  status?: string;
  applicationDocuments?: ApplicationDocumentLink[];
  benefits?: BenefitDetail[];
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