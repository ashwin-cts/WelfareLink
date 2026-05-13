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
  auditorAPi: string;
  citizenDocumentApi: string;
  eligibilityApi: string;
  welfareApplicationApi: string;
  benefitApi: string;
  benefitAnalyticsApi: string;
  welfareAnalyticsApi: string;
  disbursementApi: string; 
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
  citizenDocumentApi: 'https://localhost:7114/api/CitizenDocumentApi',
  programApi: 'https://localhost:7029/api/WelfareProgramApi',
  resourceApi: 'https://localhost:7029/api/ResourceApi',
  auditorAPi: 'https://localhost:7129/api/GovernmentAuditorApi',
  eligibilityApi: 'https://localhost:7143/api/EligibilityCheckApi',
  welfareApplicationApi: 'https://localhost:7143/api/WelfareApplicationApi',
  benefitApi: 'https://localhost:7143/api/BenefitApi',
  benefitAnalyticsApi: 'https://localhost:7143/api/BenefitAnalyticsApi',
  welfareAnalyticsApi: 'https://localhost:7143/api/WelfareApplicationAnalyticsApi',
  disbursementApi: 'https://localhost:7143/api/DisbursementApi',
};