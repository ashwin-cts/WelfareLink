import { Component, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

import { AuthService } from '../services/auth.service';
import { AuthResponse, AuthErrorResponse } from '../models/auth.model';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
  loginForm: FormGroup;
  isLoading = false;
  errorMessage = '';

  userRoles = [
    { id: 'Citizen', label: 'Citizen' },
    { id: 'Admin', label: 'System Administrator' },
    { id: 'ProgramManager', label: 'Program Manager' },
    { id: 'WelfareOfficer', label: 'Welfare Officer' },
    { id: 'ComplianceOfficer', label: 'Compliance Officer' },
    { id: 'GovernmentAuditor', label: 'Government Auditor' },
  ];

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {
    this.loginForm = this.fb.group({
      userType: ['Citizen', Validators.required],
      username: ['', Validators.required],
      password: ['', Validators.required]
    });
  }

  get isCitizenSelected(): boolean {
    return this.loginForm.get('userType')?.value === 'Citizen';
  }

  onSubmit() {
    if (this.loginForm.invalid) return;

    this.isLoading = true;
    this.errorMessage = '';
    this.cdr.detectChanges();

    this.authService.login(this.loginForm.value).subscribe({
      next: (res: any) => { // Changed strictly for flexibility with the C# response casing
        this.isLoading = false;
        this.cdr.detectChanges();

        const validToken = res.token || res.Token;
        if (validToken) {
          localStorage.setItem('token', validToken);
        }

        // --- NEW LINES ADDED HERE ---
        // Safely grab the role and name regardless of how the C# API capitalized it
        const role = res.role || res.Role || this.loginForm.value.userType;
        const name = res.fullName || res.FullName || res.username || res.Username || this.loginForm.value.username;

        localStorage.setItem('userRole', role);
        localStorage.setItem('userName', name);
        // ----------------------------

        if (role === 'Admin') {
          this.router.navigate(['/admin-dashboard']);
        }
        else if (role === 'ProgramManager') {
          this.router.navigate(['/program-manager/dashboard']);
        }
        else {
          this.router.navigate(['/dashboard']);
        }
      },

      error: (err: HttpErrorResponse) => {
        this.isLoading = false;
        const errData = err.error as AuthErrorResponse;

        if (errData && errData.error) {
          this.errorMessage = errData.error;
        }
        else if (errData && errData.errors) {
          const firstErrorKey = Object.keys(errData.errors)[0];
          this.errorMessage = errData.errors[firstErrorKey][0];
        }
        else if (errData && errData.Error) {
          this.errorMessage = errData.Error;
        }
        else {
          this.errorMessage = 'Invalid username or password. Please try again.';
        }

        this.cdr.detectChanges();
      }
    });
  }
}