import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

// 1. Import the PM Navbar
import { ProgramManagerNavbarComponent } from '../program-manager-navbar.component/program-manager-navbar.component';
// 2. Import the Reusable Account Components
import { ChangePasswordComponent } from '../../../account/components/change-password.component/change-password.component';
import { EditProfileComponent } from '../../../account/components/edit-profile.component/edit-profile.component';

@Component({
  selector: 'app-pm-profile',
  standalone: true,
  imports: [
    CommonModule, 
    RouterModule, 
    ProgramManagerNavbarComponent, 
    ChangePasswordComponent, 
    EditProfileComponent
  ],
  template: `
    <app-program-manager-navbar></app-program-manager-navbar>
    
    <div class="container mt-4 mb-5">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h1 class="h3 mb-0 text-gray-800"><i class="bi bi-person-badge"></i> Account Settings</h1>
          <p class="text-muted">Manage your profile details and security credentials.</p>
        </div>
        <a routerLink="/program-manager/dashboard" class="btn btn-secondary shadow-sm">
            <i class="bi bi-arrow-left"></i> Back to Dashboard
        </a>
      </div>

      <div class="row">
        <div class="col-md-6 mb-4">
          <div class="card shadow-sm border-0 h-100">
            <div class="card-body">
              <app-edit-profile></app-edit-profile>
            </div>
          </div>
        </div>

        <div class="col-md-6 mb-4">
          <div class="card shadow-sm border-0 h-100">
            <div class="card-body">
              <app-change-password></app-change-password>
            </div>
          </div>
        </div>
      </div>
    </div>
  `
})
export class PmProfileComponent { }