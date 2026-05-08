import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';

import { AdminService } from '../../../admin-dashboard/services/admin.service';
import { ChangePasswordRequest } from '../../../admin-dashboard/models/admin.model';
import { ProgramManagerNavbarComponent } from '../../../program-manager/components/program-manager-navbar.component/program-manager-navbar.component';

@Component({
  selector: 'app-change-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ProgramManagerNavbarComponent],
  templateUrl: './change-password.component.html'
})
export class ChangePasswordComponent implements OnInit {
  passwordForm: FormGroup;
  currentUserId: number | null = null;
  isLoading = false;
  successMessage = '';
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private adminService: AdminService,
    private cdr: ChangeDetectorRef
  ) {
    this.passwordForm = this.fb.group({
      currentPassword: ['', Validators.required],
      newPassword: ['', [Validators.required, Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9\s])\S{8,}$/)]],
      confirmPassword: ['', Validators.required]
    });
  }

  ngOnInit() {
    this.extractUserIdFromToken();
  }

  extractUserIdFromToken() {
    const token = localStorage.getItem('token') || localStorage.getItem('jwt');
    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        const nameIdentifier = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
        this.currentUserId = Number(payload.UserId || payload.sub || nameIdentifier);
      } catch (e) {
        this.errorMessage = "Session error. Please log in again.";
      }
    }
  }

  onSubmit() {
    if (this.passwordForm.invalid || !this.currentUserId) return;

    const vals = this.passwordForm.value;
    if (vals.newPassword !== vals.confirmPassword) {
      this.errorMessage = "New passwords do not match!";
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const payload: ChangePasswordRequest = {
      currentPassword: vals.currentPassword,
      newPassword: vals.newPassword
    };

    this.adminService.changePassword(this.currentUserId, payload).subscribe({
      next: () => {
        this.isLoading = false;
        this.successMessage = 'Password updated successfully!';
        this.passwordForm.reset();
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.Error || err.error?.message || 'Failed to update password.';
        this.cdr.detectChanges();
      }
    });
  }
}