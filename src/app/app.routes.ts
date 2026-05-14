import { Routes } from '@angular/router';

import { Login } from './features/auth/login/components/login';
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
import { ComplianceOfficer } from './features/compliance-officer/components/compliance-officer/compliance-officer';
import { ComplianceApplicationDetailsComponent } from './features/compliance-officer/components/application-details.component/compliance-application-details.component';

// Admin & Auth
import { AdminProfileComponent } from './features/admin-dashboard/components/admin-profile-component/admin-profile-component';
import { authGuard } from './core/guards/auth-guard';

// Citizen Dashboard
import { CitizenDashboard } from './features/citizen/components/dashboard/citizen-dashboard/citizen-dashboard';

export const routes: Routes = [
  { path: 'login', component: Login },

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
    path: 'compliance-dashboard',
    component: ComplianceOfficer,
    canActivate: [authGuard]
  },
  {
    path: 'compliance-dashboard/application/:id',
    component: ComplianceApplicationDetailsComponent,
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