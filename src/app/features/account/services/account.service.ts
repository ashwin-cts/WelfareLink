import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_CONFIG, ApiConfig } from '../../../core/config/api.config';
import { AccountProfile, ChangePasswordRequest, UpdateProfileRequest, UserProfile, ApiResponse } from '../models/account.model';
@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private apiUrl: string;

  constructor(
    private http: HttpClient,
    @Inject(API_CONFIG) private apiConfig: ApiConfig
  ) {
    this.apiUrl = this.apiConfig.userApi;
  }

  getProfile(userId: number): Observable<AccountProfile> {
    return this.http.get<AccountProfile>(`${this.apiUrl}/${userId}`);
  }
  updateProfile(userId: number, profileData: UpdateProfileRequest): Observable<UserProfile> {
    return this.http.put<UserProfile>(`${this.apiConfig.userApi}/${userId}/profile`, profileData);
  }

  changePassword(userId: number, data: ChangePasswordRequest): Observable<any> {
    return this.http.put(`${this.apiUrl}/${userId}/password`, data);
  }
}