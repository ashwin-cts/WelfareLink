import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { API_CONFIG, ApiConfig } from '../../../core/config/api.config';

import { ComplianceFinding, ComplianceMetrics, AccountProfile, ChangePasswordRequest, UpdateProfileRequest, UserProfile, ApiResponse, SystemLog, PaginatedLogs, CreateUserRequest, ComplianceRecord, ComplianceRecordCreateRequest, AuditEntry, AuditCreateRequest, NotificationItem, DashboardApplication, ApplicationDetail, ProgramResourcesDto } from '../models/compliance-officer';

@Injectable({
  providedIn: 'root',
})
export class ComplianceOfficerService {
  constructor(
    private http: HttpClient,
    @Inject(API_CONFIG) private apiConfig: ApiConfig
  ) {}

  getMetrics(): Observable<ComplianceMetrics> {
    return this.http.get<ComplianceMetrics>(`${this.apiConfig.complianceApi}/metrics`);
  }

  getIssues(): Observable<ComplianceFinding[]> {
    return this.http.get<ComplianceFinding[]>(`${this.apiConfig.complianceApi}/issues`);
  }

  getAllocations(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiConfig.complianceApi}/allocations`);
  }

  getAllApplicationsWithBenefits(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiConfig.complianceApi}/applications`);
  }

  getBenefitAllocations(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiConfig.complianceApi}/allocations`);
  }

  resolveIssue(recordId: number, notes: string): Observable<any> {
    return this.http.put<any>(`${this.apiConfig.complianceApi}/resolve/${recordId}`, { notes });
  }

  raiseComplianceForAllocation(benefitId: number, description: string): Observable<any> {
    return this.http.post<any>(`${this.apiConfig.complianceApi}/raise-compliance-allocation?benefitID=${benefitId}`, { description });
  }

  getComplianceRecords(): Observable<ComplianceRecord[]> {
    return this.http.get<ComplianceRecord[]>(`${this.apiConfig.complianceApi}/records`);
  }

  getDashboardApplications(): Observable<DashboardApplication[]> {
    return this.http
      .get<{ success: boolean; count: number; data: DashboardApplication[] }>(`${this.apiConfig.complianceApi}/dashboard/applications-list`)
      .pipe(map((res) => res.data || []));
  }

  getApplicationDetails(applicationId: number): Observable<ApplicationDetail> {
    return this.http.get<ApplicationDetail>(`${this.apiConfig.citizenApi}CitizenApi/application/${applicationId}`);
  }

  getProgramResources(programId: number): Observable<ProgramResourcesDto> {
    return this.http.get<ProgramResourcesDto>(`${this.apiConfig.resourceApi}/program/${programId}`);
  }

  createComplianceRecord(record: ComplianceRecordCreateRequest): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(`${this.apiConfig.complianceApi}/records`, record);
  }

  getAudits(): Observable<AuditEntry[]> {
    return this.http.get<AuditEntry[]>(`${this.apiConfig.auditApi}/audits`);
  }

  createAudit(audit: AuditCreateRequest): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(`${this.apiConfig.auditApi}/audits`, audit);
  }

  getNotifications(): Observable<NotificationItem[]> {
    return this.http.get<NotificationItem[]>(`${this.apiConfig.complianceApi}/notifications`);
  }

  markNotificationRead(notificationId: number): Observable<ApiResponse> {
    return this.http.put<ApiResponse>(`${this.apiConfig.complianceApi}/notifications/${notificationId}/read`, {});
  }

  // Account service methods
  getProfile(userId: number): Observable<AccountProfile> {
    return this.http.get<AccountProfile>(`${this.apiConfig.userApi}/${userId}`);
  }
  updateProfile(userId: number, profileData: UpdateProfileRequest): Observable<UserProfile> {
    return this.http.put<UserProfile>(`${this.apiConfig.userApi}/${userId}/profile`, profileData);
  }

  changePassword(userId: number, data: ChangePasswordRequest): Observable<any> {
    return this.http.put(`${this.apiConfig.userApi}/${userId}/password`, data);
  }

  // Admin service methods
  getUsers(): Observable<UserProfile[]> {
    return this.http.get<UserProfile[]>(`${this.apiConfig.adminApi}/users`);
  }

  blockUser(userId: number): Observable<ApiResponse> {
    return this.http.put<ApiResponse>(`${this.apiConfig.adminApi}/${userId}/block`, {});
  }

  unblockUser(userId: number): Observable<ApiResponse> {
    return this.http.put<ApiResponse>(`${this.apiConfig.adminApi}/${userId}/unblock`, {});
  }

  createOfficer(userData: CreateUserRequest): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(`${this.apiConfig.adminApi}/create-officer`, userData);
  }

  createAdmin(userData: CreateUserRequest): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(`${this.apiConfig.adminApi}/create-admin`, userData);
  }

  getSystemLogs(pageNumber: number, pageSize: number): Observable<PaginatedLogs> {
    return this.http.get<PaginatedLogs>(`${this.apiConfig.auditApi}/paged?pageNumber=${pageNumber}&pageSize=${pageSize}`);
  }

}
