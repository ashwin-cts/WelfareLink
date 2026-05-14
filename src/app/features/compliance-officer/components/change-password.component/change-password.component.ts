import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';

import { ChangePasswordRequest } from '../../models/compliance-officer';
import { ComplianceOfficerService } from '../../services/compliance-officer';

@Component({
  selector: 'app-change-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './change-password.component.html',
  styleUrls: ['./change-password.component.css'] // Make sure this file exists, even if empty
})
export class ChangePasswordComponent implements OnInit {
  passwordForm: FormGroup;
  currentUserId: number | null = null;
  isLoading = false;
  successMessage = '';
  errorMessage = '';

  showCurrent = false;
  showNew = false;
  showConfirm = false;

  constructor(
    private fb: FormBuilder,
    private complianceService: ComplianceOfficerService,
    private cdr: ChangeDetectorRef
  ) {
    this.passwordForm = this.fb.group({
      currentPassword: ['', Validators.required],
      newPassword: ['', [Validators.required, Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9\s])\S{8,}$/)]],
      confirmPassword: ['', Validators.required]
    }, { validators: this.passwordMatchValidator });
  }

  ngOnInit() {
    this.extractUserIdFromToken();
  }

  passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const newPassword = control.get('newPassword')?.value;
    const confirmPassword = control.get('confirmPassword')?.value;
    if (newPassword && confirmPassword && newPassword !== confirmPassword) {
      return { passwordMismatch: true };
    }
    return null;
  }

  toggleVisibility(field: 'current' | 'new' | 'confirm') {
    if (field === 'current') this.showCurrent = !this.showCurrent;
    if (field === 'new') this.showNew = !this.showNew;
    if (field === 'confirm') this.showConfirm = !this.showConfirm;
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

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const payload: ChangePasswordRequest = {
      currentPassword: this.passwordForm.value.currentPassword,
      newPassword: this.passwordForm.value.newPassword
    };

    this.complianceService.changePassword(this.currentUserId, payload).subscribe({
      next: () => {
        this.isLoading = false;
        this.successMessage = 'Password updated successfully!';
        this.passwordForm.reset();
        this.showCurrent = false;
        this.showNew = false;
        this.showConfirm = false;
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