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
  documentID: number;           // Replaced 'id'
  docType: string;              // Replaced 'documentType'
  uploadDate: string;
  verificationStatus: string;   // Replaced 'status'
  remarks?: string;
}

export interface WelfareProgram {
  programID: number;            // Replaced 'id'
  title: string;
  description?: string;
  duration: string;
  budget: number;
  eligibleGender?: string;
  requiredDocuments: string;    // Replaced 'requiredDocs'
  status: string;
}

export interface WelfareApplication {
  applicationID: number;        // Replaced 'id'
  programID: number;            // Replaced 'programName'
  submittedDate: string;        // Replaced 'applicationDate'
  status: string;
}

export interface CitizenDashboardStats {
  pendingDocuments: number;
  approvedDocuments: number;
  rejectedDocuments: number;
  documents: CitizenDocument[];
}
// Add these to your existing citizen.model.ts file

export interface CitizenProfile {
  id: number;
  username: string;
  name: string;
  email: string;
  dateOfBirth: string;
  address: string;
  contactInfo: string;
  gender: string;
}

export interface UpdateCitizenProfileRequest {
  name: string;
  email: string;
  contactInfo: string;
  address: string;
}

export interface ApplyProgramRequest {
  citizenID: number;             // Matched to C# uppercase 'ID'
  programID: number;             // Matched to C# uppercase 'ID'
  selectedDocumentIds?: number[]; // Added the array for the documents!
}
// A generic response for when C# just returns { "message": "Success!" }
export interface ApiResponse {
  message?: string;
  success?: boolean;
}