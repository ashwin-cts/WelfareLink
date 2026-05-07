import { Routes } from '@angular/router';

import { Login } from './features/auth/login/components/login'; 
// Make sure this path matches where you moved AdminDashboard!
import { AdminDashboard } from './features/admin-dashboard/components/admin-dashboard'; 

// 1. IMPORT THE NEW CITIZEN DASHBOARD
import { CitizenDashboard } from './features/citizen/components/dashboard/citizen-dashboard/citizen-dashboard';

import { authGuard } from './core/guards/auth-guard'; 

export const routes: Routes = [
  { path: 'login', component: Login },
  
  { 
    path: 'admin-dashboard', 
    component: AdminDashboard,
    canActivate: [authGuard] 
  },

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