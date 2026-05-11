import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, map } from 'rxjs'; 
import { WelfareApplication, EligibilityCheck } from '../models/welfare-officer models';

@Injectable({
  providedIn: 'root'
})
export class WelfareOfficerService {
  private apiUrl = 'https://localhost:7143/api/WelfareApplicationApi';
  private eligibilityApiUrl = 'https://localhost:7143/api/EligibilityCheckApi';

  constructor(private http: HttpClient) { }

  /* --- APPLICATION METHODS --- */

  getApplications(): Observable<WelfareApplication[]> {
    // No manual mapping needed! The API and Model match perfectly now.
    return this.http.get<WelfareApplication[]>(this.apiUrl);
  }

  getPendingApplications(): Observable<WelfareApplication[]> {
    return this.getApplications().pipe(
      map(applications => applications.filter(app => app.status === 'Pending'))
    );
  }

  getApplicationById(id: number): Observable<WelfareApplication> {
    return this.http.get<WelfareApplication>(`${this.apiUrl}/${id}`);
  }

  updateApplicationStatus(id: number, status: string): Observable<any> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    const body = JSON.stringify(status); 
    return this.http.patch(`${this.apiUrl}/${id}/status`, body, { headers });
  }

  deleteApplication(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  /* --- ELIGIBILITY CHECK METHODS --- */

  getAllEligibilityChecks(): Observable<EligibilityCheck[]> {
    return this.http.get<EligibilityCheck[]>(this.eligibilityApiUrl).pipe(
      map(checks => checks.map(c => ({
        ...c,
        // Normalizing the ID: Backend might send applicationID or application_id
        applicationId: c.applicationId || (c as any).applicationID || (c as any).application_id
      })))
    );
  }

  getEligibilityCheckById(id: number): Observable<EligibilityCheck> {
    return this.http.get<EligibilityCheck>(`${this.eligibilityApiUrl}/${id}`).pipe(
      map(c => ({
        ...c,
        applicationId: c.applicationId || (c as any).applicationID || (c as any).application_id
      }))
    );
  }

  updateEligibilityCheck(id: number, check: EligibilityCheck): Observable<any> {
    return this.http.put(`${this.eligibilityApiUrl}/${id}`, check);
  }

  getEligibilityHistory(applicationId: number): Observable<EligibilityCheck[]> {
    return this.http.get<EligibilityCheck[]>(`${this.eligibilityApiUrl}/application/${applicationId}`);
  }
}