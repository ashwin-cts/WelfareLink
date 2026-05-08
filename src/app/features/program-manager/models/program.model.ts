export interface WelfareProgram {
    programID: number;           // Matches API Schema
    title: string;
    description: string;
    startDate: string;           // Use string for ISO dates from JSON
    endDate: string;
    budget: number;
    maxBenefitPerCitizen: number;
    status: string;
    eligibleGender: string;
    requiredDocuments: string;
}

export interface Resource {
    resourceID: number;          // Matches API Schema
    programID: number;
    type: string;
    quantity: number;
    status: string;
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