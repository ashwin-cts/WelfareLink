import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_CONFIG } from '../../../core/config/api.config';
import { Disbursement, BenefitDetails } from '../models/disbursement.model';

@Injectable({
  providedIn: 'root'
})
export class DisbursementService {
  private http = inject(HttpClient);
  private config = inject(API_CONFIG);

  // --- Core CRUD Operations ---

  getDisbursements(): Observable<Disbursement[]> {
    return this.http.get<Disbursement[]>(this.config.disbursementApi);
  }

  getDisbursementById(id: number): Observable<Disbursement> {
    return this.http.get<Disbursement>(`${this.config.disbursementApi}/${id}`);
  }

  createDisbursement(disbursement: Partial<Disbursement>): Observable<Disbursement> {
    return this.http.post<Disbursement>(this.config.disbursementApi, disbursement);
  }

  updateDisbursement(id: number, disbursement: Partial<Disbursement>): Observable<void> {
    return this.http.put<void>(`${this.config.disbursementApi}/${id}`, disbursement);
  }

  deleteDisbursement(id: number): Observable<void> {
    return this.http.delete<void>(`${this.config.disbursementApi}/${id}`);
  }

  // --- Specialized Endpoints ---

  getPendingDisbursements(): Observable<Disbursement[]> {
    return this.http.get<Disbursement[]>(`${this.config.disbursementApi}/pending`);
  }

  getDisbursementHistory(): Observable<Disbursement[]> {
    return this.http.get<Disbursement[]>(`${this.config.disbursementApi}/history`);
  }

  // Used in the History Component for the advanced search form
  filterDisbursements(filters: any): Observable<Disbursement[]> {
    let params = new HttpParams();
    
    if (filters.startDate) params = params.set('startDate', filters.startDate);
    if (filters.endDate) params = params.set('endDate', filters.endDate);
    if (filters.benefitType) params = params.set('benefitType', filters.benefitType);
    if (filters.officerId) params = params.set('officerId', filters.officerId);
    if (filters.status) params = params.set('status', filters.status);

    return this.http.get<Disbursement[]>(`${this.config.disbursementApi}/filter`, { params });
  }

  // --- Benefit Integration (Used in Create/Edit Forms) ---

  getBenefitDetails(benefitId: number): Observable<BenefitDetails> {
    return this.http.get<BenefitDetails>(`${this.config.disbursementApi}/benefit-details/${benefitId}`);
  }

  getSiblingDisbursements(benefitId: number): Observable<Disbursement[]> {
    return this.http.get<Disbursement[]>(`${this.config.disbursementApi}/benefit/${benefitId}`);
  }
}