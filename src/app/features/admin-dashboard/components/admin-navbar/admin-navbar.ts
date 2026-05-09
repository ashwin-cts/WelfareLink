import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-admin-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './admin-navbar.html',
  styleUrls: ['./admin-navbar.css'] // Create an empty CSS file for this if you don't have one
})
export class AdminNavbarComponent implements OnInit {
  public router = inject(Router);

  userName = signal<string>('Admin User');
  userRole = signal<string>('Admin');

  ngOnInit(): void {
    const savedName = localStorage.getItem('userName');
    const savedRole = localStorage.getItem('userRole');

    if (savedName) this.userName.set(savedName);
    if (savedRole) this.userRole.set(savedRole);
  }
  goToDashboard(event: Event) {
    event.preventDefault(); // Stops the page from jumping
    
    if (this.router.url === '/admin-dashboard') {
      // If already on the dashboard, force a quick reload to reset the tabs
      this.router.navigateByUrl('/', { skipLocationChange: true }).then(() => {
          this.router.navigate(['/admin-dashboard']);
      });
    } else {
      // If on the Profile page, just navigate normally
      this.router.navigate(['/admin-dashboard']);
    }
  }

  logout() {
    localStorage.clear();
    this.router.navigate(['/login']);
  }
}