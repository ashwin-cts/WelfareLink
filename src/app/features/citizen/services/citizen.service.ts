import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_CONFIG, ApiConfig } from '../../../core/config/api.config';

// 1. Import all the strict types, including the new ones we just made!
import { 
  CreateCitizenRequest, 
  CitizenDocument, 
  WelfareProgram, 
  WelfareApplication, 
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
  // Add this inside CitizenService
  changePassword(citizenId: number, passwordData: any): Observable<ApiResponse> {
    return this.http.put<ApiResponse>(`${this.apiConfig.citizenApi}/CitizenApi/${citizenId}/password`, passwordData);
  }

  // --- DOCUMENTS ---
  getDocuments(citizenId: number): Observable<CitizenDocument[]> {
    return this.http.get<CitizenDocument[]>(`${this.apiConfig.citizenApi}/CitizenDocumentApi/citizen/${citizenId}`);
  }

  uploadDocument(formData: FormData): Observable<ApiResponse> {
    // FormData remains FormData because it is a native browser object for files
    return this.http.post<ApiResponse>(`${this.apiConfig.citizenApi}/CitizenDocumentApi/upload`, formData);
  }

  deleteDocument(docId: number): Observable<ApiResponse> {
    return this.http.delete<ApiResponse>(`${this.apiConfig.citizenApi}/CitizenDocumentApi/${docId}`);
  }

  // --- PROGRAMS & APPLICATIONS ---
  getPrograms(): Observable<WelfareProgram[]> {
    return this.http.get<WelfareProgram[]>(`${this.apiConfig.programApi}/WelfareProgramApi`);
  }

  getApplications(citizenId: number): Observable<WelfareApplication[]> {
    return this.http.get<WelfareApplication[]>(`${this.apiConfig.citizenApi}/CitizenApi/${citizenId}/applications`);
  }

  applyForProgram(applicationData: ApplyProgramRequest): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(`${this.apiConfig.citizenApi}/CitizenApi/apply`, applicationData);
  }
}