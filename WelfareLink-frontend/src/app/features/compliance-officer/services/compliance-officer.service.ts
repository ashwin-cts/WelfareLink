import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { API_CONFIG, ApiConfig } from '../../../core/config/api.config';
import { ComplianceMetrics, DashboardApplication, ApplicationDetail, ComplianceRecord,EligibilityCheck } from '../models/compliance-officer.model';

@Injectable({
  providedIn: 'root',
})
export class ComplianceOfficerService {
  constructor(
    private http: HttpClient,
    @Inject(API_CONFIG) private apiConfig: ApiConfig
  ) {}

  getDashboardApplications(): Observable<DashboardApplication[]> {
    return this.http
      .get<{ data: DashboardApplication[] }>(`${this.apiConfig.complianceApi}/ComplianceOfficerDashboardApi/dashboard/applications-list`)
      .pipe(map((res) => res.data || []));
  }

  getApplicationDetails(applicationId: number): Observable<ApplicationDetail> {
    return this.http.get<ApplicationDetail>(`${this.apiConfig.citizenApi}/CitizenApi/application/${applicationId}`);
  }

  getComplianceRecords(): Observable<ComplianceRecord[]> {
    return this.http.get<ComplianceRecord[]>(`${this.apiConfig.complianceApi}/ComplainceRecordApi`);
  }

  raiseComplianceForAllocation(benefitId: number, payload: { violationType: string, description: string }): Observable<any> {
    return this.http.post<any>(`${this.apiConfig.complianceApi}/ComplianceOfficerDashboardApi/raise-compliance-allocation?benefitID=${benefitId}`, payload);
  }

  raiseComplianceForDisbursement(disbursementId: number, payload: { violationType: string, description: string }): Observable<any> {
    return this.http.post<any>(`${this.apiConfig.complianceApi}/ComplianceOfficerDashboardApi/raise-compliance-disbursement?disbursementID=${disbursementId}`, payload);
  }

  resolveComplianceIssue(recordId: number, notes: string): Observable<any> {
    return this.http.put<any>(`${this.apiConfig.complianceApi}/ComplianceOfficerDashboardApi/resolve/${recordId}`, { notes });
  }
  getDocumentFile(docId: number): Observable<Blob> {
    return this.http.get(`${this.apiConfig.citizenApi}/CitizenDocumentApi/${docId}/file`, {
      responseType: 'blob'
    });
  }
  getLatestEligibilityCheck(applicationId: number): Observable<EligibilityCheck> {
    
    return this.http.get<EligibilityCheck>(`${this.apiConfig.eligibilityApi}/application/${applicationId}/latest`);
  }
}