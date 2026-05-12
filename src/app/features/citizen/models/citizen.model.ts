export interface CreateCitizenRequest {
  username: string;
  password?: string;
  name: string;
  email: string;
  dateOfBirth: string;
  address: string;
  contactInfo: string;
  gender: string;
}

export interface CitizenDocument {
  documentID: number;            
  citizenId?: number;            // Added from your image
  docType: string;               
  documentName?: string;         
  fileURI?: string;              // Added from your image
  uploadedDate: string;          // FIXED: Was uploadDate, now matches backend
  verificationStatus: string;   
  remarks?: string;
}

export interface WelfareProgram {
  programID: number;             
  title: string;
  description?: string;
  startDate: string;         // Added to match API response
  endDate: string;           // Added to match API response 
  duration?: string;       
  budget: number;
  eligibleGender?: string;   // Lowercase 'e'
  requiredDocuments: string; // Lowercase 'r'
  status: string;
}

export interface CitizenDashboardStats {
  pendingDocuments: number;
  approvedDocuments: number;
  rejectedDocuments: number;
  documents: CitizenDocument[];
}

export interface CitizenProfile {
  citizenId?: number; // We need this to update the profile!
  userId?: number;
  username: string;
  name: string;
  email: string;
  dateOfBirth: string;
  address: string;
  contactInfo: string;
  gender: string;
  status?: string;
  createdAt?: string;
}

export interface UpdateCitizenProfileRequest {
  citizenId?: number; // Adding this to fix the ID Mismatch!
  name: string;
  email: string;
  contactInfo: string;
  address: string;
}

export interface ApplyProgramRequest {
  citizenID: number;             
  programID: number;             
  selectedDocumentIds?: number[]; 
}

export interface ApiResponse {
  message?: string;
  success?: boolean;
}