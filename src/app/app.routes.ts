import { Routes } from '@angular/router';

// Make sure these paths exactly match your actual file names!
// Usually Angular adds '.component' to the file name.
import { Login } from './features/auth/login/login'; 
import { AdminDashboard } from './features/admin-dashboard/admin-dashboard';

// Import our new Bouncer
import { authGuard } from './core/guards/auth-guard'; 

export const routes: Routes = [
  { path: 'login', component: Login },
  
  { 
    path: 'admin-dashboard', 
    component: AdminDashboard,
    canActivate: [authGuard] // <--- The Guard is now protecting this route!
  },
//   anyone can type localhost:4200/admin-dashboard into their browser and see your dashboard,
//  even if they aren't logged in! (The API calls will fail, but they will still see the empty HTML page).
// To fix this, Angular uses things called Auth Guards.
  { path: '', redirectTo: '/login', pathMatch: 'full' }, 
  
  // The Wildcard: If they type a URL that doesn't exist, send them to login
  { path: '**', redirectTo: '/login' } 
];