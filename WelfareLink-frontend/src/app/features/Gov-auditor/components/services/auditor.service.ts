import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_CONFIG, ApiConfig } from '../../../../core/config/api.config';
// IMPORT the models from your new file (Adjust the path if necessary)
import { 
  AuditorDashboardStats, 
  BudgetMonitoringItem, 
  ResourceStatementItem, 
  DisbursementStatementItem 
} from '../../models/auditor.model';

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