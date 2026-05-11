// 1. Program & Budget Level (Where the money starts)
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

// 2. Citizen Level (Who receives the money)
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

// 3. Disbursement Level (The actual money sent)
export interface Disbursement {
  disbursementID: number;
  benefitID: number;
  citizenID: number;
  officerID: number;
  amount: number;
  date: string;
  status: string;
}

// 4. Benefit Level (The approved amount before disbursement)
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

// 5. Application Level (Tying it all together for the Auditor)
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

// 6. Auditor Specific Dashboard Summaries (Mapped to your Auditor API endpoints)
export interface AuditorDashboardSummary {
  totalBudget: number;
  totalUtilized: number;
  totalDisbursed: number;
  totalPendingBenefits: number;
  activeProgramsCount: number;
}