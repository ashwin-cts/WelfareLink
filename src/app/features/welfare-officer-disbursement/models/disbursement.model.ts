export interface Disbursement {
  disbursementID: number;
  benefitID: number;
  citizenID: number;
  officerID: number;
  amount: number;
  date: string; // Stored as ISO string (e.g., '2026-05-12T00:00:00Z')
  status: string | null;
  
  // Optional navigation properties if your API returns them in the GET list
  benefit?: any; 
}

export interface BenefitDetails {
  benefitType: string | null;
  benefitAmount: number | null;
  programTitle: string | null;
  citizenName: string | null;
  citizenId: number | null;
  totalResource: number;
  totalDisbursedForProgram: number;
  availableResource: number;
  isResourceExhausted: boolean;
}