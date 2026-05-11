import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-welfare-application-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './welfare-application-navbar.component.html',
  styleUrls: ['./welfare-application-navbar.component.css']
})
export class WelfareApplicationNavbarComponent implements OnInit {
  // 1. Made router public so HTML can read the active URL
  public router = inject(Router);

  // 2. Signals to hold API-driven user data
  userName = signal<string>('Loading...');
  userRole = signal<string>('Welfare Officer');

  ngOnInit(): void {
    // These values should be saved in localStorage after a successful API login
    const savedName = localStorage.getItem('userName');
    const savedRole = localStorage.getItem('userRole');

    if (savedName) {
      this.userName.set(savedName);
    } else {
      this.userName.set('Officer John Doe'); // Fallback
    }

    if (savedRole) {
      this.userRole.set(savedRole);
    }
  }

  // 3. The Force Reload trick for the Welfare Officer Dashboard
  goToDashboard(event: Event) {
    event.preventDefault();
    if (this.router.url === '/welfare-officer/dashboard') {
      this.router.navigateByUrl('/', { skipLocationChange: true }).then(() => {
          this.router.navigate(['/welfare-officer/dashboard']);
      });
    } else {
      this.router.navigate(['/welfare-officer/dashboard']);
    }
  }

  // 4. Secure Logout Logic
  logout() {
    // Clears session to prevent "Invalid Login" cache issues
    localStorage.clear();
    this.router.navigate(['/login']);
  }
}