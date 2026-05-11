import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth-guard';

// ==========================================
// 1. AUTH & BASE COMPONENTS
// ==========================================
import { Login } from './features/auth/login/components/login';

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
// 4. PROGRAM MANAGER COMPONENTS
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
// Upcoming imports (we will build these next)
// import { CitizenProgramsComponent } from './features/citizen/components/citizen-programs.component/citizen-programs.component';
// import { CitizenApplicationsComponent } from './features/citizen/components/citizen-applications.component/citizen-applications.component';
// import { CitizenDocumentsComponent } from './features/citizen/components/citizen-documents.component/citizen-documents.component';

export const routes: Routes = [
  // --- Public Routes ---
  { path: 'login', component: Login },

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
  // Upcoming Citizen Routes (uncomment as we build them)
  // { path: 'citizen/programs', component: CitizenProgramsComponent, canActivate: [authGuard] },
  // { path: 'citizen/my-applications', component: CitizenApplicationsComponent, canActivate: [authGuard] },
  // { path: 'citizen/documents', component: CitizenDocumentsComponent, canActivate: [authGuard] },

  // --- Fallback Routes ---
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: '**', redirectTo: '/login' }
];