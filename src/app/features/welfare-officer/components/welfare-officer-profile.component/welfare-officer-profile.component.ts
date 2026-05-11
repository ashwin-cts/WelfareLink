import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

// 1. Import the Welfare Officer Navbar
import { WelfareApplicationNavbarComponent } from '../welfare-application-navbar.component/welfare-application-navbar.component';

// 2. Import the Reusable Account Components (adjust the relative paths if necessary)
import { ChangePasswordComponent } from '../../../account/components/change-password.component/change-password.component';
import { EditProfileComponent } from '../../../account/components/edit-profile.component/edit-profile.component';

@Component({
  selector: 'app-welfare-officer-profile',
  standalone: true,
  imports: [
    CommonModule, 
    RouterModule, 
    WelfareApplicationNavbarComponent, 
    ChangePasswordComponent, 
    EditProfileComponent
  ],
  template: `
    <app-welfare-application-navbar></app-welfare-application-navbar>
    
    <div class="container mt-4 mb-5">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h1 class="h3 mb-0 text-navy fw-bold"><i class="bi bi-person-badge"></i> Account Settings</h1>
          <p class="text-muted">Manage your profile details and security credentials.</p>
        </div>
        <a routerLink="/welfare-officer/dashboard" class="btn btn-secondary shadow-sm">
            <i class="bi bi-arrow-left"></i> Back to Dashboard
        </a>
      </div>

      <div class="row">
        <div class="col-md-6 mb-4">
          <div class="card shadow-sm border-0 h-100 rounded-4 overflow-hidden">
            <div class="card-body p-0">
              <app-edit-profile></app-edit-profile>
            </div>
          </div>
        </div>

        <div class="col-md-6 mb-4">
          <div class="card shadow-sm border-0 h-100 rounded-4 overflow-hidden">
            <div class="card-body p-0">
              <app-change-password></app-change-password>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .text-navy {
        color: #1a2a4d !important;
    }
  `]
})
export class WelfareOfficerProfileComponent { }