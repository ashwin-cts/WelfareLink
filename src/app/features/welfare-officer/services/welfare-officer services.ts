import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, map } from 'rxjs'; 
import { API_CONFIG } from '../../../core/config/api.config';
import { WelfareApplication, EligibilityCheck } from '../models/welfare-officer models';

@Injectable({
  providedIn: 'root'
})
export class WelfareOfficerService {
  // 1. Switched to inject() to match ProgramManagerService perfectly
  private http = inject(HttpClient);
  private config = inject(API_CONFIG);

  /* --- APPLICATION METHODS --- */

  getApplications(): Observable<WelfareApplication[]> {
    return this.http.get<WelfareApplication[]>(this.config.welfareApplicationApi);
  }

  getPendingApplications(): Observable<WelfareApplication[]> {
    return this.getApplications().pipe(
      map(applications => applications.filter(app => app.status === 'Pending'))
    );
  }

  getApplicationById(id: number): Observable<WelfareApplication> {
    return this.http.get<WelfareApplication>(`${this.config.welfareApplicationApi}/${id}`);
  }

  updateApplicationStatus(id: number, status: string): Observable<void> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    const body = JSON.stringify(status); 
    return this.http.patch<void>(`${this.config.welfareApplicationApi}/${id}/status`, body, { headers });
  }

  deleteApplication(id: number): Observable<void> {
    return this.http.delete<void>(`${this.config.welfareApplicationApi}/${id}`);
  }

  /* --- ELIGIBILITY CHECK METHODS --- */

  // 1. GET: /api/EligibilityCheckApi
  getAllEligibilityChecks(): Observable<EligibilityCheck[]> {
    return this.http.get<EligibilityCheck[]>(this.config.eligibilityApi).pipe(
      map(checks => checks.map(c => ({
        ...c,
        applicationId: c.applicationID || (c as unknown as { applicationID: number }).applicationID
      })))
    );
  }

  // 2. GET: /api/EligibilityCheckApi/{id}
  getEligibilityCheckById(id: number): Observable<EligibilityCheck> {
    return this.http.get<EligibilityCheck>(`${this.config.eligibilityApi}/${id}`).pipe(
      map(c => ({
        ...c,
        applicationId: c.applicationID || (c as unknown as { applicationID: number }).applicationID
      }))
    );
  }

  // 3. POST: /api/EligibilityCheckApi (with applicationId query parameter)
  createEligibilityCheck(check: EligibilityCheck, applicationId?: number): Observable<EligibilityCheck> {
    let url = this.config.eligibilityApi;
    if (applicationId) {
      url += `?applicationId=${applicationId}`;
    }
    return this.http.post<EligibilityCheck>(url, check);
  }

  // 4. PUT: /api/EligibilityCheckApi/{id}
  updateEligibilityCheck(id: number, check: EligibilityCheck): Observable<EligibilityCheck> {
    return this.http.put<EligibilityCheck>(`${this.config.eligibilityApi}/${id}`, check);
  }

  // 5. DELETE: /api/EligibilityCheckApi/{id}
  deleteEligibilityCheck(id: number): Observable<void> {
    return this.http.delete<void>(`${this.config.eligibilityApi}/${id}`);
  }

  // 6. GET: /api/EligibilityCheckApi/application/{applicationId}
  getEligibilityHistory(applicationId: number): Observable<EligibilityCheck[]> {
    return this.http.get<EligibilityCheck[]>(`${this.config.eligibilityApi}/application/${applicationId}`);
  }

  // 7. GET: /api/EligibilityCheckApi/application/{applicationId}/latest
  getLatestEligibilityCheck(applicationId: number): Observable<EligibilityCheck> {
    return this.http.get<EligibilityCheck>(`${this.config.eligibilityApi}/application/${applicationId}/latest`);
  }

  // 8. GET: /api/EligibilityCheckApi/summary
  getEligibilitySummary(): Observable<unknown> {
    return this.http.get<unknown>(`${this.config.eligibilityApi}/summary`);
  }

  // 9. GET: /api/EligibilityCheckApi/application-info/{applicationId}
  getApplicationInfoForCheck(applicationId: number): Observable<unknown> {
    return this.http.get<unknown>(`${this.config.eligibilityApi}/application-info/${applicationId}`);
  }

  /* --- DOCUMENT METHODS --- */

  // 10. PATCH: /api/CitizencitizenDocumentApi/{id}/verify
  updateDocumentStatus(documentId: number, status: string): Observable<void> {
    const url = `${this.config.citizenDocumentApi}/${documentId}/verify`;
    
    // This perfectly mimics JsonSerializer.Serialize(status) from your C# code
    const body = JSON.stringify(status); 
    
    // This perfectly mimics Encoding.UTF8, "application/json"
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    
    return this.http.patch<void>(url, body, { headers });
  }
}