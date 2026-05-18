import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth-guard';
import { HomeComponent } from './features/home-component/home.component';
// ==========================================
// 1. AUTH & ACCOUNT COMPONENTS
// ==========================================
import { Login } from './features/auth/login/components/login';
import { EditProfileComponent } from './features/account/components/edit-profile.component/edit-profile.component';
import { ChangePasswordComponent } from './features/account/components/change-password.component/change-password.component';

// ==========================================
// 2. ADMIN COMPONENTS
// ==========================================
import { AdminDashboard } from './features/admin-dashboard/components/admin-dashboard';
import { AdminProfileComponent } from './features/admin-dashboard/components/admin-profile-component/admin-profile-component';

// ==========================================
// 3. AUDITOR COMPONENTS
// ==========================================
import { AuditorDashboardComponent } from './features/Gov-auditor/components/auditor-dashboard.component/auditor-dashboard.component';
import { AuditorProfileComponent } from './features/Gov-auditor/components/auditor-profile.component/auditor-profile.component';

// ==========================================
// 4. PROGRAM MANAGER & RESOURCE COMPONENTS
// ==========================================
import { PmDashboardComponent } from './features/program-manager/components/pm-dashboard.component/pm-dashboard.component';
import { PmProfileComponent } from './features/program-manager/components/pm-profile.component/pm-profile.component';
import { ProgramDetailsComponent } from './features/program-manager/components/program-details.component/program-details.component';
import { ProgramListComponent } from './features/program-manager/components/program-list.component/program-list.component';
import { ProgramFormComponent } from './features/program-manager/components/program-form.component/program-form.component';
import { BudgetStatsComponent } from './features/program-manager/components/budget-stats.component/budget-stats.component';
import { PerformanceMetricsComponent } from './features/program-manager/components/performance-metrics.component/performance-metrics.component';
import { ResourceListComponent } from './features/program-manager/components/resource-list.component/resource-list.component';
import { ManageResourcesComponent } from './features/program-manager/components/manage-resources.component/manage-resources.component';
import { ResourceFormComponent } from './features/program-manager/components/resource-form.component/resource-form.component';
import { UtilisationReportComponent } from './features/program-manager/components/utilisation-report.component/utilisation-report.component';

// ==========================================
// 5. CITIZEN COMPONENTS
// ==========================================
import { CitizenDashboardComponent } from './features/citizen/components/citizen-dashboard/citizen-dashboard.component';
import { CitizenProfileComponent } from './features/citizen/components/citizen-profile.component/citizen-profile.component';
import { ApplicationDetailsComponent } from './features/citizen/components/application-details.component/application-details.component';
import { CitizenDocumentListComponent } from './features/citizen/components/citizen-document-list.component/citizen-document-list.component';
import { CitizenDocumentFormComponent } from './features/citizen/components/citizen-document-form.component/citizen-document-form.component';
import { CitizenProgramListComponent } from './features/citizen/components/citizen-programs.component/citizen-program-list.component';
import { CitizenApplyFormComponent } from './features/citizen/components/citizen-programs.component/citizen-apply-form.component/citizen-apply-form.component';
import { CitizenApplicationsComponent } from './features/citizen/components/citizen-applications.component/citizen-applications.component';

// ==========================================
// 6. WELFARE OFFICER COMPONENTS
// ==========================================
import { DashboardComponent } from "./features/welfare-officer/components/dashboard/dashboard.component";
import { DetailsComponent } from './features/welfare-officer/components/details/details.component';
import { WelfareOfficerProfileComponent } from './features/welfare-officer/components/welfare-officer-profile.component/welfare-officer-profile.component';
import { WelfareApplicationAnalyticsComponent } from './features/welfare-officer/components/welfare-application-analytics.component/welfare-application-analytics.component';

// Eligibility
import { EligibilityListComponent } from './features/welfare-officer/components/eligibility-list/eligibility-list.component';
import { EligibilityDetailsComponent } from './features/welfare-officer/components/eligibility-details.component/eligibility-details.component';
import { EligibilityFormComponent } from './features/welfare-officer/components/eligibility-form.component/eligibility-form.component';

// Benefit
import { BenefitListComponent } from './features/welfare-officer-benefit/components/benefit-list.component/benefit-list.component';
import { BenefitDetailsComponent } from './features/welfare-officer-benefit/components/benefit-details.component/benefit-details.component';
import { BenefitFormComponent } from './features/welfare-officer-benefit/components/benefit-form.component/benefit-form.component';
import { BenefitAnalyticsComponent } from './features/welfare-officer-benefit/components/benefit-analytics.component/benefit-analytics.component';

// Disbursement
import { DisbursementListComponent } from './features/welfare-officer-disbursement/components/disbursement-list.component/disbursement-list.component';
import { DisbursementFormComponent } from './features/welfare-officer-disbursement/components/disbursement-form.component/disbursement-form.component';
import { DisbursementHistoryComponent } from './features/welfare-officer-disbursement/components/disbursement-history.component/disbursement-history.component';
import { DisbursementDetailComponent } from './features/welfare-officer-disbursement/components/disbursement-details.component/disbursement-details.component';
// 7 Compliance
import { ComplianceOfficerProfileComponent } from './features/compliance-officer/components/compliance-officer-profile.component/compliance-officer-profile.component';
import { ComplianceDashboardComponent } from './features/compliance-officer/components/compliance-officer-dashboard.component/compliance-officer-dashboard.component';
import { ComplianceApplicationDetailsComponent } from './features/compliance-officer/components/compliance-officer-application-list.component/compliance-officer-application-list.component';
import { FlagIssueComponent } from './features/compliance-officer/components/compliance-officer-flag.component/compliance-officer-flag.component';
import { ComplianceRecordsComponent } from './features/compliance-officer/components/compliance-officer-records.component/compliance-officer-records.component';
//import { ComplianceRecordsComponent } from './features/compliance-officer/components/compliance-officer-records.component/compliance-officer-records.component';

export const routes: Routes = [
  

  // --- Account Management Routes ---
  { path: 'account/edit-profile', component: EditProfileComponent, canActivate: [authGuard] },
  { path: 'account/change-password', component: ChangePasswordComponent, canActivate: [authGuard] },

  // --- Admin Routes ---
  { path: 'admin-dashboard', component: AdminDashboard, canActivate: [authGuard] },
  { path: 'admin/profile', component: AdminProfileComponent, canActivate: [authGuard] },

  // --- Auditor Routes ---
  { path: 'auditor-dashboard', component: AuditorDashboardComponent, canActivate: [authGuard] },
  { path: 'auditor/profile', component: AuditorProfileComponent, canActivate: [authGuard] },

  // --- Program Manager Routes ---
  { path: 'program-manager/dashboard', component: PmDashboardComponent, canActivate: [authGuard] },
  { path: 'program-manager/profile', component: PmProfileComponent, canActivate: [authGuard] },
  { path: 'program-manager/details/:id', component: ProgramDetailsComponent, canActivate: [authGuard] },
  { path: 'program-manager/list', component: ProgramListComponent, canActivate: [authGuard] },
  { path: 'program-manager/create', component: ProgramFormComponent, canActivate: [authGuard] },
  { path: 'program-manager/edit/:id', component: ProgramFormComponent, canActivate: [authGuard] },
  { path: 'program-manager/budget', component: BudgetStatsComponent, canActivate: [authGuard] },
  { path: 'program-manager/performance', component: PerformanceMetricsComponent, canActivate: [authGuard] },
  
  // --- Resource Manager Routes ---
  { path: 'resource-manager', component: ResourceListComponent, canActivate: [authGuard] },
  { path: 'resource-manager/program/:id', component: ManageResourcesComponent, canActivate: [authGuard] },
  { path: 'resource-manager/allocate', component: ResourceFormComponent, canActivate: [authGuard] },
  { path: 'resource-manager/edit/:id', component: ResourceFormComponent, canActivate: [authGuard] },
  { path: 'resource-manager/utilisation', component: UtilisationReportComponent, canActivate: [authGuard] },

  // --- Citizen Routes ---
  { path: 'citizen-dashboard', component: CitizenDashboardComponent, canActivate: [authGuard] },
  { path: 'citizen/profile', component: CitizenProfileComponent, canActivate: [authGuard] },
  { path: 'citizen/application/:id', component: ApplicationDetailsComponent, canActivate: [authGuard] },
  { path: 'citizen/documents', component: CitizenDocumentListComponent, canActivate: [authGuard] },
  { path: 'citizen/documents/upload', component: CitizenDocumentFormComponent, canActivate: [authGuard] },
  { path: 'citizen/programs', component: CitizenProgramListComponent, canActivate: [authGuard] },
  { path: 'citizen/programs/apply/:id', component: CitizenApplyFormComponent, canActivate: [authGuard] },
  { path: 'citizen/my-applications', component: CitizenApplicationsComponent, canActivate: [authGuard] },

  // --- Welfare Officer Routes ---
  { path: 'welfare-officer/dashboard', component: DashboardComponent, canActivate: [authGuard] },
  { path: 'welfare-officer/profile', component: WelfareOfficerProfileComponent, canActivate: [authGuard] },
  { path: 'details/:id', component: DetailsComponent, canActivate: [authGuard] },
  { path: 'welfare-officer/welfare-application-analytics', component: WelfareApplicationAnalyticsComponent, canActivate: [authGuard] },

  // --- Eligibility Routes ---
  { path: 'eligibility-list', component: EligibilityListComponent, canActivate: [authGuard] },
  { path: 'eligibility-details/:id', component: EligibilityDetailsComponent, canActivate: [authGuard] },
  { path: 'eligibility-create', component: EligibilityFormComponent, canActivate: [authGuard] },
  { path: 'eligibility-edit/:id', component: EligibilityFormComponent, canActivate: [authGuard] },

  // --- Benefit Routes ---
  { path: 'welfare-officer/benefit-list', component: BenefitListComponent, canActivate: [authGuard] },
  { path: 'welfare-officer/benefit-details/:id', component: BenefitDetailsComponent, canActivate: [authGuard] },
  { path: 'welfare-officer/benefit-create', component: BenefitFormComponent, canActivate: [authGuard] },
  { path: 'welfare-officer/benefit-edit/:id', component: BenefitFormComponent, canActivate: [authGuard] },
  { path: 'welfare-officer/benefit-analytics', component: BenefitAnalyticsComponent, canActivate: [authGuard] },

  // --- Disbursement Routes ---
  { path: 'welfare-officer/disbursement-list', component: DisbursementListComponent, canActivate: [authGuard] },
  { path: 'welfare-officer/disbursement-create', component: DisbursementFormComponent, canActivate: [authGuard] },
  { path: 'welfare-officer/disbursement-edit/:id', component: DisbursementFormComponent, canActivate: [authGuard] },
  { path: 'welfare-officer/disbursement-details/:id', component: DisbursementDetailComponent, canActivate: [authGuard] },
  { path: 'welfare-officer/disbursement-history', component: DisbursementHistoryComponent, canActivate: [authGuard] },

  // --- Compliance Routes ---
{ 
    path: 'compliance', 
    component: ComplianceOfficerProfileComponent, 
    canActivate: [authGuard],
    children: [
      { path: 'dashboard', component: ComplianceDashboardComponent },
      { path: 'application/:id', component: ComplianceApplicationDetailsComponent },
      { path: 'flag-issue/:id', component: FlagIssueComponent },
      { path: 'records', component: ComplianceRecordsComponent },
      { path: 'edit-profile', component: EditProfileComponent }, // Reusing your shared account component
      { path: 'change-password', component: ChangePasswordComponent }, // Reusing your shared account component
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
    ]
  },
  // --- Fallback Routes ---
  //{ path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'home', component: HomeComponent },
  //default
   {path: '', component: HomeComponent, pathMatch: 'full' },
   { path: 'login', redirectTo: '/home', pathMatch: 'full' },
  // { path: '**', redirectTo: '/login' }
  // Catch-all wildcard route: Redirects any unknown URLs back to the Home page
{ path: '**', redirectTo: '', pathMatch: 'full' }
];