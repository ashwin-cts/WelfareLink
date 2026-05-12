import { Routes } from '@angular/router';

// 1. Core Imports
import { Login } from './features/auth/login/components/login';

// 2. Account & Profile Imports
import { EditProfileComponent } from './features/account/components/edit-profile.component/edit-profile.component';
import { ChangePasswordComponent } from './features/account/components/change-password.component/change-password.component';

// 3. Admin & Program Manager Imports
import { AdminDashboard } from './features/admin-dashboard/components/admin-dashboard';
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
import { WelfareOfficerProfileComponent } from './features/welfare-officer/components/welfare-officer-profile.component/welfare-officer-profile.component';
import { EligibilityFormComponent } from './features/welfare-officer/components/eligibility-form.component/eligibility-form.component';
import { WelfareApplicationAnalyticsComponent } from './features/welfare-officer/components/welfare-application-analytics.component/welfare-application-analytics.component';

// Admin & Auth
import { AdminProfileComponent } from './features/admin-dashboard/components/admin-profile-component/admin-profile-component';
import { authGuard } from './core/guards/auth-guard';

// 4. Welfare Officer Imports
import { DashboardComponent } from "./features/welfare-officer/components/dashboard/dashboard.component";
import { DetailsComponent } from './features/welfare-officer/components/details/details.component';

// 5. Eligibility Section Imports (Unified)
import { EligibilityListComponent } from './features/welfare-officer/components/eligibility-list/eligibility-list.component';
import { EligibilityDetailsComponent } from './features/welfare-officer/components/eligibility-details.component/eligibility-details.component';

// 6. Citizen Imports
// Citizen Dashboard
import { CitizenDashboard } from './features/citizen/components/dashboard/citizen-dashboard/citizen-dashboard';

//Benefit
import { BenefitListComponent } from './features/welfare-officer-benefit/components/benefit-list.component/benefit-list.component';
import { BenefitDetailsComponent } from './features/welfare-officer-benefit/components/benefit-details.component/benefit-details.component';
import { BenefitFormComponent } from './features/welfare-officer-benefit/components/benefit-form.component/benefit-form.component';
import { BenefitAnalyticsComponent } from './features/welfare-officer-benefit/components/benefit-analytics.component/benefit-analytics.component'
// disbursement
import { DisbursementListComponent } from './features/welfare-officer-disbursement/components/disbursement-list.component/disbursement-list.component';
import { DisbursementFormComponent } from './features/welfare-officer-disbursement/components/disbursement-form.component/disbursement-form.component';
import { DisbursementHistoryComponent } from './features/welfare-officer-disbursement/components/disbursement-history.component/disbursement-history.component';
import { DisbursementDetailComponent } from './features/welfare-officer-disbursement/components/disbursement-details.component/disbursement-details.component';
export const routes: Routes = [
  // Authentication
  { path: 'login', component: Login },
  { path: '', redirectTo: '/login', pathMatch: 'full' },

  // Admin
  { path: 'admin-dashboard', component: AdminDashboard, canActivate: [authGuard] },

  // Welfare Officer Section
  { path: 'welfare-officer/dashboard', component: DashboardComponent, canActivate: [authGuard] },

  { path: 'details/:id', component: DetailsComponent, canActivate: [authGuard] },

  // Eligibility Section
  { path: 'eligibility-list', component: EligibilityListComponent, canActivate: [authGuard] },
  { path: 'eligibility-details/:id', component: EligibilityDetailsComponent, canActivate: [authGuard] },
  // For creating a new check (POST)
  { path: 'eligibility-create', component: EligibilityFormComponent },

  // For editing an existing check (PUT)
  { path: 'eligibility-edit/:id', component: EligibilityFormComponent },

  // For editing an existing check:
  { path: 'eligibility-edit/:id', component: EligibilityFormComponent },
  // Program Manager Section
  { path: 'program-manager/dashboard', component: PmDashboardComponent, canActivate: [authGuard] },
  { path: 'program-manager/details/:id', component: ProgramDetailsComponent, canActivate: [authGuard] },
  { path: 'welfare-officer/benefit-list', component: BenefitListComponent, canActivate: [authGuard] },
  { path: 'welfare-officer/benefit-details/:id', component: BenefitDetailsComponent, canActivate: [authGuard] },
  { path: 'welfare-officer/benefit-create', component: BenefitFormComponent, canActivate: [authGuard] },
  { path: 'welfare-officer/benefit-edit/:id', component: BenefitFormComponent, canActivate: [authGuard] },
  { path: 'welfare-officer/benefit-analytics', component: BenefitAnalyticsComponent, canActivate: [authGuard] },
  { path: 'welfare-officer/welfare-application-analytics', component: WelfareApplicationAnalyticsComponent, canActivate: [authGuard] },
  {path:'welfare-officer/disbursement-list', component: DisbursementListComponent, canActivate: [authGuard]},
  {path:'welfare-officer/disbursement-create', component: DisbursementFormComponent, canActivate: [authGuard]},
  {path:'welfare-officer/disbursement-edit/:id', component: DisbursementFormComponent, canActivate: [authGuard]},
  {path:'welfare-officer/disbursement-details/:id', component: DisbursementDetailComponent, canActivate: [authGuard]},
  {path:'welfare-officer/disbursement-history', component: DisbursementHistoryComponent, canActivate: [authGuard]},
  
  // Citizen Section
  { path: 'citizen-dashboard', component: CitizenDashboard, canActivate: [authGuard] },

  // Account Management
  { path: 'account/edit-profile', component: EditProfileComponent, canActivate: [authGuard] },
  { path: 'account/change-password', component: ChangePasswordComponent, canActivate: [authGuard] },

  {
    path: 'welfare-officer/profile',
    component: WelfareOfficerProfileComponent
  },
  // Catch-all redirect

  {
    path: 'admin-dashboard',
    component: AdminDashboard,
    canActivate: [authGuard]
  },
  {
    path: 'admin/profile',
    component: AdminProfileComponent,
    canActivate: [authGuard]
  },
  {
    path: 'program-manager/dashboard',
    component: PmDashboardComponent,
    canActivate: [authGuard]
  },
  {
    path: 'program-manager/profile', // <--- This matches our updated HTML now!
    component: PmProfileComponent,
    canActivate: [authGuard]
  },
  {
    path: 'resource-manager/program/:id',
    component: ManageResourcesComponent,
    canActivate: [authGuard]
  },
  {
    path: 'resource-manager/allocate',
    component: ResourceFormComponent,
    canActivate: [authGuard]
  },
  {
    path: 'resource-manager/edit/:id',
    component: ResourceFormComponent,
    canActivate: [authGuard]
  },
  {
    path: 'resource-manager/utilisation',
    component: UtilisationReportComponent,
    canActivate: [authGuard]
  },
  {
    path: 'program-manager/details/:id',
    component: ProgramDetailsComponent,
    canActivate: [authGuard]
  },
  {
    path: 'program-manager/list',
    component: ProgramListComponent,
    canActivate: [authGuard]
  },
  {
    path: 'resource-manager',
    component: ResourceListComponent,
    canActivate: [authGuard]
  },
  {
    path: 'program-manager/create',
    component: ProgramFormComponent,
    canActivate: [authGuard]
  },
  {
    path: 'program-manager/budget',
    component: BudgetStatsComponent,
    canActivate: [authGuard]
  },
  {
    path: 'program-manager/performance',
    component: PerformanceMetricsComponent,
    canActivate: [authGuard]
  },
  {
    path: 'program-manager/edit/:id',
    component: ProgramFormComponent,
    canActivate: [authGuard]
  },
  {
    path: 'citizen-dashboard',
    component: CitizenDashboard,
    canActivate: [authGuard]
  },

  // Base Redirect
  { path: '', redirectTo: '/login', pathMatch: 'full' },

  // The Wildcard: If they type a URL that doesn't exist, send them to login
  { path: '**', redirectTo: '/login' }
];