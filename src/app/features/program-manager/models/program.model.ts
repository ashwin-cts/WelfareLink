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

// Helper interface for Budget Monitoring endpoint
export interface BudgetMonitoring {
    programName: string;
    allocatedBudget: number;
    spentAmount: number;
    remainingBudget: number;
}