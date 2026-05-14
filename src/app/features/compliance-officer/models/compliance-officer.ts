export interface ComplianceMetrics {
	total: number;
	open: number;
	resolved: number;
	issuesByType?: Array<{ violationType: string; count: number }>;
}

export interface ComplianceFinding {
	recordID: number;
	entityType: string;
	entityId: number;
	violationType: string;
	description: string;
	status: string;
	createdDate: string;
	resolvedDate?: string;
	applicationID?: number;
	citizenID?: number;
	raisedBy?: { userId: number; username: string };
	notes?: string;
}

export interface DashboardApplication {
  ApplicationID: number;
  CitizenName: string;
  CitizenID: number;
  ProgramTitle: string;
  ProgramID: number;
  ApplicationStatus: string;
  submittedDate: string;
  MaxBenefit?: number;
  TotalBenefitAllocated?: number;
  TotalDisbursed?: number;
  RemainingToDisborse?: number;
  BenefitCount?: number;
  DisbursementCount?: number;
  IsPendingAllocation?: boolean;
  HasNoDisbursement?: boolean;
  IsFlagged?: boolean;
}

export interface ApplicationDetail {
  ApplicationID: number;
  CitizenID: number;
  citizen?: {
    CitizenId: number;
    Name: string;
    Email?: string;
  };
  ProgramID: number;
  program?: {
    ProgramID: number;
    Title: string;
    MaxBenefitPerCitizen?: number;
  };
  SubmittedDate: string;
  Status: string;
  Benefits?: BenefitDetail[];
  ApplicationDocuments?: ApplicationDocumentLink[];
}

export interface BenefitDetail {
  BenefitID: number;
  Type: string;
  Amount: number;
  Date: string;
  Status: string;
  Disbursements?: DisbursementDetail[];
}

export interface DisbursementDetail {
  DisbursementID: number;
  amount: number;
  date: string;
  status: string;
}

export interface ApplicationDocumentLink {
  id: number;
  applicationID: number;
  documentID: number;
  CitizenDocument?: CitizenDocumentLink;
}

export interface CitizenDocumentLink {
  DocumentID: number;
  DocType: string;
  DocumentName: string;
  UploadedDate: string;
}

export interface ProgramResourcesDto {
  programTitle: string;
  programBudget: number;
  totalAllocated: number;
  remainingBudget: number;
  resources: ResourceDto[];
}

export interface ResourceDto {
  resourceID: number;
  name: string;
  amountAllocated: number;
}

export interface AccountProfile {
    fullName: string;
    email: string;
}
export interface UserProfile {
    userId?: number;
    username: string;
    fullName?: string;
    email?: string;
    role?: string;
    isActive?: boolean;
}

export interface UpdateProfileRequest {
    fullName?: string;
    email?: string;
}

export interface ChangePasswordRequest {
    currentPassword: string;
    newPassword: string;
}

// Reusable standard API response
export interface ApiResponse {
    message?: string;
    success?: boolean;
}

export interface SystemLog {
  timestamp: string;
  userName: string;
  action: string;
  entityType: string;
  entityId: string;
  description: string;
  ipAddress: string;
}

export interface PaginatedLogs {
  items?: SystemLog[];
  data?: SystemLog[];
  records?: SystemLog[];
  pageNumber?: number;
  currentPage?: number;
  totalPages?: number;
}


export interface CreateUserRequest {
  username: string;
  password?: string;
  role: string;
  fullName?: string;
  email?: string;
}

export interface ComplianceRecord {
  complianceId: number;
  entityId: number;
  entityType: 'Application' | 'Benefit' | 'Program' | 'Other';
  result: string;
  date: string;
  notes?: string;
  status: string;
}

export interface ComplianceRecordCreateRequest {
  entityId: number;
  entityType: 'Application' | 'Benefit' | 'Program' | 'Other';
  result: string;
  notes?: string;
}

export interface AuditEntry {
  auditId: number;
  officerId: number;
  scope: string;
  findings: string;
  date: string;
  status: string;
}

export interface AuditCreateRequest {
  scope: string;
  findings: string;
  status: string;
}

export interface NotificationItem {
  notificationId: number;
  userId: number;
  entityId: number;
  message: string;
  category: 'Application' | 'Benefit' | 'Program' | 'Compliance' | 'Audit';
  status: 'Unread' | 'Read';
  createdDate: string;
}
