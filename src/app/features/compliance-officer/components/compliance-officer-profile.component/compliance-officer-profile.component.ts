import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';

@Component({
  selector: 'app-compliance-officer-profile',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <nav class="navbar navbar-expand-lg navbar-dark bg-primary mb-4 shadow-sm">
      <div class="container">
        <a class="navbar-brand fw-bold" routerLink="/compliance/dashboard">
          <i class="bi bi-shield-check me-2"></i>WelfareLink Compliance
        </a>
        <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav">
          <span class="navbar-toggler-icon"></span>
        </button>
        <div class="collapse navbar-collapse" id="navbarNav">
          <ul class="navbar-nav ms-auto align-items-center">
            <li class="nav-item">
              <a class="nav-link" routerLink="/compliance/dashboard" routerLinkActive="active"><i class="bi bi-speedometer2 me-1"></i>Dashboard</a>
            </li>
            <li class="nav-item">
              <a class="nav-link" routerLink="/compliance/records" routerLinkActive="active"><i class="bi bi-check-circle me-1"></i>Records</a>
            </li>
            
            <li class="nav-item dropdown" (clickOutside)="isAccountDropdownOpen = false">
              <a class="nav-link dropdown-toggle" href="javascript:void(0)" (click)="toggleDropdown($event)">
                <i class="bi bi-gear-fill me-1"></i>Account
              </a>
              <ul class="dropdown-menu dropdown-menu-end shadow" [class.show]="isAccountDropdownOpen">
                <li><a class="dropdown-item" routerLink="/compliance/edit-profile" (click)="isAccountDropdownOpen = false"><i class="bi bi-person me-2"></i>Edit Profile</a></li>
                <li><a class="dropdown-item" routerLink="/compliance/change-password" (click)="isAccountDropdownOpen = false"><i class="bi bi-key me-2"></i>Change Password</a></li>
                <li><hr class="dropdown-divider"></li>
                <li><button class="dropdown-item text-danger" (click)="logout()"><i class="bi bi-box-arrow-right me-2"></i>Logout</button></li>
              </ul>
            </li>
          </ul>
        </div>
      </div>
    </nav>
    <div class="container py-4">
      <router-outlet></router-outlet>
    </div>
  `
})
export class ComplianceOfficerProfileComponent {
  isAccountDropdownOpen = false;

  constructor(private router: Router) {}

  toggleDropdown(event: Event) {
    event.preventDefault();
    this.isAccountDropdownOpen = !this.isAccountDropdownOpen;
  }

  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('jwt');
    localStorage.removeItem('userRole');
    this.router.navigate(['/login']);
  }
}