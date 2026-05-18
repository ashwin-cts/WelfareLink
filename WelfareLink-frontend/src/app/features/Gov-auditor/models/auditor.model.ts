// ==========================================
// AUDITOR DASHBOARD API MODELS
// ==========================================
export interface AuditorDashboardStats {
  totalApplications: number;
  totalPrograms: number;
  totalBudget: number;
  totalResource: number;
  totalDisbursement: number;
}

export interface BudgetMonitoringItem {
  programName: string;
  programStatus: string;
  programBudget: number;
  allocatedResource: number;
  citizensApplied: number;
  totalDisbursed: number;
  remainingResource: number;
  utilizationPercent: number;
}

export interface ResourceStatementItem {
  date: string;
  resourceID: number;
  programName: string;
  allocatedResource: number;
  remainingAllocationPending: number;
}

export interface DisbursementStatementItem {
  CitizenID: number;
  CitizenName: string;
  MaxBenefit: number;
  BenefitAllocated: number;
  Disbursed: number;
  RemainDisburse: number;
  DisbursementPercent: number;
}

export interface AuditorDashboardSummary {
  totalBudget: number;
  totalUtilized: number;
  totalDisbursed: number;
  totalPendingBenefits: number;
  activeProgramsCount: number;
}

// ==========================================
// CORE ENTITY MODELS
// ==========================================
export interface WelfareProgram {
  programID: number;
  title: string;
  description: string;
  startDate: string;
  endDate: string;
  budget: number;
  maxBenefitPerCitizen: number;
  status: string;
  eligibleGender: string;
  requiredDocuments: string;
}

export interface Citizen {
  citizenId: number;
  userId: number;
  name: string;
  dateOfBirth: string;
  address: string;
  contactInfo: string;
  status: string;
  gender: string;
  createdAt: string;
}

export interface Disbursement {
  disbursementID: number;
  benefitID: number;
  citizenID: number;
  officerID: number;
  amount: number;
  date: string;
  status: string;
}

export interface Benefit {
  benefitID: number;
  applicationID: number;
  type: string;
  amount: number;
  date: string;
  status: string;
  disbursements?: Disbursement[];
}

export interface EligibilityCheck {
  checkID: number;
  applicationID: number;
  officerID: number;
  result: string;
  resultCode: string;
  date: string;
  notes: string;
}

export interface CitizenDocument {
  documentID: number;
  citizenId: number;
  docType: string;
  documentName: string;
  fileURI: string;
  uploadedDate: string;
  verificationStatus: string;
}

export interface ApplicationDocument {
  id: number;
  applicationID: number;
  documentID: number;
  citizenDocument: CitizenDocument;
}

export interface WelfareApplication {
  applicationID: number;
  citizenID: number;
  programID: number;
  submittedDate: string;
  status: string;
  eligibilityChecks?: EligibilityCheck[];
  benefits?: Benefit[];
  program?: WelfareProgram;
  citizen?: Citizen;
  applicationDocuments?: ApplicationDocument[];
}