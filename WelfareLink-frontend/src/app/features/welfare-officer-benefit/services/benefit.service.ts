// src/app/features/benefit/services/benefit.service.ts

import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { defaultApiConfig } from '../../../core/config/api.config';
import { Benefit, ProgramResourceInfo } from '../models/benefit.model';
import { AnalyticsDashboardViewModel } from '../models/benefit.model';

@Injectable({
  providedIn: 'root'
})
export class BenefitService {
  private http = inject(HttpClient);
  // Pointing to the BenefitApi URL we added in api.config.ts
  private apiUrl = defaultApiConfig.benefitApi;
  private benefitAnalyticsApiUrl = defaultApiConfig.benefitAnalyticsApi;

  constructor() { }

  // GET: /api/BenefitApi
  getAllBenefits(): Observable<Benefit[]> {
    return this.http.get<Benefit[]>(this.apiUrl);
  }

  // GET: /api/BenefitApi/{id}
  getBenefitById(id: number): Observable<Benefit> {
    return this.http.get<Benefit>(`${this.apiUrl}/${id}`);
  }
  getApplicationById(appId: number): Observable<any> {
    // You can replace the hardcoded URL with your apiConfig variable later!
    return this.http.get<any>(`https://localhost:7143/api/WelfareApplicationApi/${appId}`);
  }
  // POST: /api/BenefitApi?officerId={id}
  createBenefit(benefit: Benefit, officerId: number = 0): Observable<Benefit> {
    // Handling the query parameter for the officer ID
    let params = new HttpParams();
    if (officerId > 0) {
      params = params.set('officerId', officerId.toString());
    }
    return this.http.post<Benefit>(this.apiUrl, benefit, { params });
  }

  // PUT: /api/BenefitApi/{id}
  updateBenefit(id: number, benefit: Benefit): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, benefit);
  }

  // DELETE: /api/BenefitApi/{id}
  deleteBenefit(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  // GET: /api/BenefitApi/filter
  // (Assuming you might pass query params for filtering later)
  filterBenefits(filters: any): Observable<Benefit[]> {
    let params = new HttpParams({ fromObject: filters });
    return this.http.get<Benefit[]>(`${this.apiUrl}/filter`, { params });
  }

  // GET: /api/BenefitApi/pending
  getPendingBenefits(): Observable<Benefit[]> {
    return this.http.get<Benefit[]>(`${this.apiUrl}/pending`);
  }

  // GET: /api/BenefitApi/dropdown
  getDropdownData(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/dropdown`);
  }

  // GET: /api/BenefitApi/program-resource-info/{programId}
  getProgramResourceInfo(programId: number): Observable<ProgramResourceInfo> {
    return this.http.get<ProgramResourceInfo>(`${this.apiUrl}/program-resource-info/${programId}`);
  }

  getAnalyticsDashboard(): Observable<AnalyticsDashboardViewModel> {
    return this.http.get<AnalyticsDashboardViewModel>(`${this.benefitAnalyticsApiUrl}/dashboard`);
  }
}