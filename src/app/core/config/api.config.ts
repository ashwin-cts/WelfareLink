import { InjectionToken } from '@angular/core';

// 1. Define the shape of our API configuration
export interface ApiConfig {
  adminApi: string;
  userApi: string;
  auditApi: string;
  authApi: string;
  citizenApi: string;
  programApi: string;
  resourceApi: string;
}

// 2. Create the Token that Angular will use to inject this config
export const API_CONFIG = new InjectionToken<ApiConfig>('API_CONFIG');

// 3. Set your default URLs here
export const defaultApiConfig: ApiConfig = {
  adminApi: 'https://localhost:7203/api/AdminApi',
  userApi: 'https://localhost:7203/api/UserApi',
  auditApi: 'https://localhost:7255/api/AuditLogApi',
  authApi: 'https://localhost:7242/api',
  citizenApi: 'https://localhost:7114/api',
  programApi: 'https://localhost:7029/api/WelfareProgramApi',
  resourceApi: 'https://localhost:7029/api/ResourceApi',


};