import { Routes } from '@angular/router';

// 1. Core Imports
import { Login } from './features/auth/login/components/login';
import { authGuard } from './core/guards/auth-guard';

// 2. Account & Profile Imports
import { EditProfileComponent } from './features/account/components/edit-profile.component/edit-profile.component';
import { ChangePasswordComponent } from './features/account/components/change-password.component/change-password.component';

// 3. Admin & Program Manager Imports
import { AdminDashboard } from './features/admin-dashboard/components/admin-dashboard';
import { PmDashboardComponent } from './features/program-manager/components/pm-dashboard.component/pm-dashboard.component';
import { ProgramDetailsComponent } from './features/program-manager/components/program-details.component/program-details.component';

// 4. Welfare Officer Imports
import { DashboardComponent } from "./features/welfare-officer/components/dashboard/dashboard.component";
import { DetailsComponent } from './features/welfare-officer/components/details/details.component';

// 5. Eligibility Section Imports (Unified)
import { EligibilityListComponent } from './features/welfare-officer/components/eligibility-list/eligibility-list.component';
import { EligibilityDetailsComponent } from './features/welfare-officer/components/eligibility-details.component/eligibility-details.component';
import { EligibilityEditComponent } from './features/welfare-officer/components/eligibility-edit.component/eligibility-edit.component';

// 6. Citizen Imports
import { CitizenDashboard } from './features/citizen/components/dashboard/citizen-dashboard/citizen-dashboard';

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
  { path: 'eligibility-edit/:id', component: EligibilityEditComponent, canActivate: [authGuard] },

  // Program Manager Section
  { path: 'program-manager/dashboard', component: PmDashboardComponent, canActivate: [authGuard] },
  { path: 'program-manager/details/:id', component: ProgramDetailsComponent, canActivate: [authGuard] },

  // Citizen Section
  { path: 'citizen-dashboard', component: CitizenDashboard, canActivate: [authGuard] },

  // Account Management
  { path: 'account/edit-profile', component: EditProfileComponent, canActivate: [authGuard] },
  { path: 'account/change-password', component: ChangePasswordComponent, canActivate: [authGuard] },

  // Catch-all redirect
  { path: '**', redirectTo: '/login' }
];