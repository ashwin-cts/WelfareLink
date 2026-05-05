import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  // Explicitly defined Microservice URLs based on your Swagger
  private readonly ADMIN_API_URL = 'https://localhost:7203/api/AdminApi';
  private readonly USER_API_URL = 'https://localhost:7203/api/UserApi';
  // Note: If you get a CORS error on logs, change this to your http port (e.g., http://localhost:5255/api/AuditLogApi)
  private readonly AUDIT_API_URL = 'https://localhost:7255/api/AuditLogApi'; 

  constructor(private http: HttpClient) { }

  // --- ADMIN ACTIONS ---
  getUsers(): Observable<any[]> {
    return this.http.get<any[]>(`${this.ADMIN_API_URL}/users`);
  }

  blockUser(userId: number): Observable<any> {
    return this.http.put(`${this.ADMIN_API_URL}/${userId}/block`, {});
  }

  unblockUser(userId: number): Observable<any> {
    return this.http.put(`${this.ADMIN_API_URL}/${userId}/unblock`, {});
  }

  createOfficer(userData: any): Observable<any> {
    return this.http.post(`${this.ADMIN_API_URL}/create-officer`, userData);
  }

  createAdmin(userData: any): Observable<any> {
    return this.http.post(`${this.ADMIN_API_URL}/create-admin`, userData);
  }

// --- SYSTEM LOGS ---
  getSystemLogs(pageNumber: number, pageSize: number): Observable<any> {
    // Added /paged to match your Swagger definition!
    return this.http.get(`${this.AUDIT_API_URL}/paged?pageNumber=${pageNumber}&pageSize=${pageSize}`);
  }
  // --- USER PROFILE ACTIONS ---
  getProfile(userId: number): Observable<any> {
    return this.http.get(`${this.USER_API_URL}/${userId}`);
  }

  updateProfile(userId: number, profileData: any): Observable<any> {
    return this.http.put(`${this.USER_API_URL}/${userId}/profile`, profileData);
  }

  changePassword(userId: number, passwordData: any): Observable<any> {
    return this.http.put(`${this.USER_API_URL}/${userId}/password`, passwordData);
  }
}