import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_CONFIG, ApiConfig } from '../../../core/config/api.config';
import { WelfareApplication } from '../../Gov-auditor/models/auditor.model';
import {
  CreateCitizenRequest,
  CitizenDocument,
  WelfareProgram,
  CitizenDashboardStats,
  CitizenProfile,
  UpdateCitizenProfileRequest,
  ApplyProgramRequest,
  ApiResponse
} from '../models/citizen.model';

@Injectable({
  providedIn: 'root'
})
export class CitizenService {
  constructor(
    private http: HttpClient,
    @Inject(API_CONFIG) private apiConfig: ApiConfig
  ) { }

  // --- REGISTRATION & PROFILE ---
  registerCitizen(data: CreateCitizenRequest): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(`${this.apiConfig.citizenApi}/CitizenApi`, data);
  }

  getProfile(userId: number): Observable<CitizenProfile> {
    return this.http.get<CitizenProfile>(`${this.apiConfig.citizenApi}/CitizenApi/by-user/${userId}`);
  }

  updateProfile(id: number, data: UpdateCitizenProfileRequest): Observable<ApiResponse> {
    return this.http.put<ApiResponse>(`${this.apiConfig.citizenApi}/CitizenApi/${id}`, data);
  }

  getDashboardStats(citizenId: number): Observable<CitizenDashboardStats> {
    return this.http.get<CitizenDashboardStats>(`${this.apiConfig.citizenApi}/CitizenApi/${citizenId}/dashboard`);
  }

  changePassword(citizenId: number, passwordData: any): Observable<ApiResponse> {
    return this.http.put<ApiResponse>(`${this.apiConfig.citizenApi}/CitizenApi/${citizenId}/password`, passwordData);
  }

  // --- DOCUMENTS ---
  getDocuments(citizenId: number): Observable<CitizenDocument[]> {
    return this.http.get<CitizenDocument[]>(`${this.apiConfig.citizenApi}/CitizenDocumentApi/citizen/${citizenId}`);
  }

  uploadDocument(formData: FormData): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(`${this.apiConfig.citizenApi}/CitizenDocumentApi/upload`, formData);
  }

  // ADDED: Re-upload method to match C# PUT endpoint
  reuploadDocument(docId: number, formData: FormData): Observable<ApiResponse> {
    return this.http.put<ApiResponse>(`${this.apiConfig.citizenApi}/CitizenDocumentApi/${docId}/reupload`, formData);
  }

  // ADDED: Helper method to safely generate the file view URL
  getDocumentFileUrl(docId: number): string {
    return `${this.apiConfig.citizenApi}/CitizenDocumentApi/${docId}/file`;
  }
  getDocumentFile(docId: number): Observable<Blob> {
    return this.http.get(`${this.apiConfig.citizenApi}/CitizenDocumentApi/${docId}/file`, {
      responseType: 'blob'
    });
  }
  deleteDocument(docId: number): Observable<ApiResponse> {
    return this.http.delete<ApiResponse>(`${this.apiConfig.citizenApi}/CitizenDocumentApi/${docId}`);
  }

  // --- PROGRAMS & APPLICATIONS ---
  getPrograms(): Observable<WelfareProgram[]> {
    return this.http.get<WelfareProgram[]>(this.apiConfig.programApi);
  }

  getApplications(citizenId: number): Observable<WelfareApplication[]> {
    return this.http.get<WelfareApplication[]>(`${this.apiConfig.citizenApi}/CitizenApi/${citizenId}/applications`);
  }

  applyForProgram(applicationData: ApplyProgramRequest): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(`${this.apiConfig.citizenApi}/CitizenApi/apply`, applicationData);
  }

  getApplicationDetails(applicationId: number): Observable<WelfareApplication> {
    return this.http.get<WelfareApplication>(`${this.apiConfig.citizenApi}/CitizenApi/application/${applicationId}`);
  }
}