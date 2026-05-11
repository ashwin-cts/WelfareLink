import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_CONFIG, ApiConfig } from '../../../../core/config/api.config';

// Define the exact shapes based on your MVC Dictionaries
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
  // Add properties based on your DisbursementStatement.cshtml
  // For example:
  date: string;
  disbursementID: number;
  citizenName: string;
  programName: string;
  amount: number;
  status: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuditorService {
  constructor(
    private http: HttpClient,
    @Inject(API_CONFIG) private config: ApiConfig
  ) {}

  getDashboardStats(): Observable<AuditorDashboardStats> {
    return this.http.get<AuditorDashboardStats>(`${this.config.auditorAPi}/dashboard`);
  }

  getBudgetMonitoring(): Observable<BudgetMonitoringItem[]> {
    return this.http.get<BudgetMonitoringItem[]>(`${this.config.auditorAPi}/budget-monitoring`);
  }

  getResourceStatement(): Observable<ResourceStatementItem[]> {
    return this.http.get<ResourceStatementItem[]>(`${this.config.auditorAPi}/resource-statement`);
  }

  getDisbursementStatement(): Observable<DisbursementStatementItem[]> {
    return this.http.get<DisbursementStatementItem[]>(`${this.config.auditorAPi}/disbursement-statement`);
  }
  
}