import { Routes } from '@angular/router';

// Make sure these paths exactly match your actual file names!
// Usually Angular adds '.component' to the file name.
import { Login } from './features/auth/login/components/login';
import { AdminDashboard } from './features/admin-dashboard/components/admin-dashboard';
import { PmDashboardComponent } from './features/program-manager/components/pm-dashboard.component/pm-dashboard.component';
import { ProgramDetailsComponent } from './features/program-manager/components/program-details.component/program-details.component';
import { EditProfileComponent } from './features/account/components/edit-profile.component/edit-profile.component';
import { ChangePasswordComponent } from './features/account/components/change-password.component/change-password.component';
// Import our new Bouncer
import { authGuard } from './core/guards/auth-guard';



// 1. IMPORT THE NEW CITIZEN DASHBOARD
import { CitizenDashboard } from './features/citizen/components/dashboard/citizen-dashboard/citizen-dashboard';


export const routes: Routes = [
  { path: 'login', component: Login },

  {
    path: 'admin-dashboard',
    component: AdminDashboard,
    canActivate: [authGuard] // <--- The Guard is now protecting this route!
  },
  {
    path: 'program-manager/dashboard',
    component: PmDashboardComponent,
    canActivate: [authGuard]
  },
  {
    path: 'program-manager/details/:id',
    component: ProgramDetailsComponent,
    canActivate: [authGuard]
  },
  {
    path: 'account/edit-profile',
    component: EditProfileComponent,
    canActivate: [authGuard]
  },
  {
    path: 'account/change-password',
    component: ChangePasswordComponent,
    canActivate: [authGuard]
  },
  //   anyone can type localhost:4200/admin-dashboard into their browser and see your dashboard,
  //  even if they aren't logged in! (The API calls will fail, but they will still see the empty HTML page).
  // To fix this, Angular uses things called Auth Guards.
  { path: '', redirectTo: '/login', pathMatch: 'full' },


  // 2. NEW ROUTE: Citizen Dashboard (Protected by the Guard)
  {
    path: 'citizen-dashboard',
    component: CitizenDashboard,
    canActivate: [authGuard]
  },

  { path: '', redirectTo: '/login', pathMatch: 'full' },

  // The Wildcard: If they type a URL that doesn't exist, send them to login
  { path: '**', redirectTo: '/login' }
];