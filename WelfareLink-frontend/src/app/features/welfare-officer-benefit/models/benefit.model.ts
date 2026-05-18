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

    // Optional navigation properties (used in your MVC Details & Index views)
    welfareApplication?: {
        applicationID: number;
        submittedDate: string;
        status: string;
        citizenID: number;
        citizen?: {
            name: string;
        };
        programID?: number;
        program?: {
            programID: number;
            title: string;
            status: string;
            budget: number;
            description: string;
        };
    };
}
export interface WelfareApplication {
    applicationID: number;
    submittedDate: string;
    status: string;

    // Flat Citizen Info
    citizenID: number;
    citizenName: string;

    // Flat Program Info
    programID: number;
    programTitle: string;
    programStatus: string;
    programDesc: string;
    programMaxBenefit: number;
    programBudget: number;
}

// export interface Benefit {
//     benefitID: number;
//     applicationID: number;
//     type: string;
//     amount: number;
//     date: string;
//     status: string;
// }

export interface ProgramResourceInfo {
    hasResource: boolean;
    totalResource: number;
    alreadyAllocated: number;
    remainingResource: number;
}
export interface Citizen {
    name: string;
}

export interface Program {
    programID: number;
    title: string;
    status: string;
    budget: number;
    description: string;
}
// Model for the "GetProgramResourceInfo" endpoint used in Create/Edit
export interface ProgramResourceInfo {
    hasResource: boolean;
    totalResource: number;
    alreadyAllocated: number;
    remainingResource: number;
}

export interface AnalyticsDashboardViewModel {
    totalAllocated: number;
    totalDisbursed: number;
    totalPending: number;
    totalFailed: number;
    totalAmountAllocated: number;
    totalAmountDisbursed: number;
    disbursementEfficiency: number;
    allocationRate: number;
    benefitTypeBreakdowns: BenefitTypeBreakdown[];
    recentDisbursements: RecentDisbursement[];
    monthlyTrends: MonthlyTrend[];
}

export interface BenefitTypeBreakdown {
    type: string;
    count: number;
    totalAmount: number;
    disbursedAmount: number;
    disbursedCount: number;
    percentage: number;
}

export interface RecentDisbursement {
    disbursementID: number;
    benefitType: string;
    citizenID: number;
    date: string;
    status: string;
}

export interface MonthlyTrend {
    month: string;
    allocated: number;
    disbursed: number;
    allocatedAmount: number;
    disbursedAmount: number;
}