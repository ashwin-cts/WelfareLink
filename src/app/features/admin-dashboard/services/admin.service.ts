import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_CONFIG, ApiConfig } from '../../../core/config/api.config';

// Import ALL your strict types
import { 
  UserProfile, 
  PaginatedLogs, 
  CreateUserRequest, 
  UpdateProfileRequest, 
  ChangePasswordRequest, 
  ApiResponse 
} from '../models/admin.model';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  
  constructor(
    private http: HttpClient,
    @Inject(API_CONFIG) private apiConfig: ApiConfig 
  ) { }

  // --- ADMIN ACTIONS ---
  getUsers(): Observable<UserProfile[]> {
    return this.http.get<UserProfile[]>(`${this.apiConfig.adminApi}/users`);
  }

  blockUser(userId: number): Observable<ApiResponse> {
    return this.http.put<ApiResponse>(`${this.apiConfig.adminApi}/${userId}/block`, {});
  }

  unblockUser(userId: number): Observable<ApiResponse> {
    return this.http.put<ApiResponse>(`${this.apiConfig.adminApi}/${userId}/unblock`, {});
  }

  // Look! No more 'any' for the userData parameters!
  createOfficer(userData: CreateUserRequest): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(`${this.apiConfig.adminApi}/create-officer`, userData);
  }

  createAdmin(userData: CreateUserRequest): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(`${this.apiConfig.adminApi}/create-admin`, userData);
  }

  // --- SYSTEM LOGS ---
  getSystemLogs(pageNumber: number, pageSize: number): Observable<PaginatedLogs> {
    return this.http.get<PaginatedLogs>(`${this.apiConfig.auditApi}/paged?pageNumber=${pageNumber}&pageSize=${pageSize}`);
  }

  // --- USER PROFILE ACTIONS ---
  getProfile(userId: number): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.apiConfig.userApi}/${userId}`);
  }

  updateProfile(userId: number, profileData: UpdateProfileRequest): Observable<UserProfile> {
    return this.http.put<UserProfile>(`${this.apiConfig.userApi}/${userId}/profile`, profileData);
  }

  changePassword(userId: number, passwordData: ChangePasswordRequest): Observable<ApiResponse> {
    return this.http.put<ApiResponse>(`${this.apiConfig.userApi}/${userId}/password`, passwordData);
  }
}