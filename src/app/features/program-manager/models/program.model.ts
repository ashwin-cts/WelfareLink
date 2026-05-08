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

export interface Resource {
    resourceID: number;
    programID: number;
    type: string;
    quantity: number;
    status: string;
    // Added to handle the included Program data from C# (e.g., item.Program.Title)
    program?: {
        title: string;
    };
    programTitle?: string; // Fallback just in case your API flattens it
}

export interface BudgetMonitoring {
    programID: number;
    programTitle: string;
    totalBudget: number;
    allocatedFunds: number;
    disbursedFunds: number;
    remainingBudget: number;
    utilisationPercentage: number;
    status: string;
    isCritical: boolean;
}

export interface ProgramPerformance {
    programID: number;
    programTitle: string;
    totalApplications: number;
    approvedApplications: number;
    rejectedApplications: number;
    pendingApplications: number;
    approvalRate: number;
    benefitsDisbursed: number;
    citizenCount: number;
    status: string;
}

// NEW: For the Utilisation Report we will build later
export interface ResourceUtilisation {
    resourceID: number;
    programID: number;
    programTitle: string;
    type: string;
    programBudget: number;
    initialQuantity: number;
    totalDisbursed: number;
    utilisationPercentage: number;
    status: string;
}