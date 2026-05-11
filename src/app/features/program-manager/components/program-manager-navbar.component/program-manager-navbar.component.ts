import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-program-manager-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './program-manager-navbar.component.html',
  styleUrls: ['./program-manager-navbar.component.css']
})
export class ProgramManagerNavbarComponent implements OnInit {
  // 1. Made router public so HTML can read the active URL
  public router = inject(Router);

  // Signals to hold API-driven user data
  userName = signal<string>('Loading...');
  userRole = signal<string>('Guest');

  ngOnInit(): void {
    // These values should be saved in localStorage after a successful API login
    const savedName = localStorage.getItem('userName');
    const savedRole = localStorage.getItem('userRole');

    if (savedName) this.userName.set(savedName);
    if (savedRole) this.userRole.set(savedRole);
  }

  // 2. Add the Force Reload trick
  goToDashboard(event: Event) {
    event.preventDefault();
    if (this.router.url === '/program-manager/dashboard') {
      this.router.navigateByUrl('/', { skipLocationChange: true }).then(() => {
          this.router.navigate(['/program-manager/dashboard']);
      });
    } else {
      this.router.navigate(['/program-manager/dashboard']);
    }
  }

  logout() {
    // Clears session to prevent "Invalid Login" cache issues
    localStorage.clear();
    this.router.navigate(['/login']);
  }
}