import { Component, ChangeDetectorRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

import { AuthService } from '../services/auth.service';
import { AuthResponse, AuthErrorResponse, RegisterCitizenRequest } from '../models/auth.model';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login implements OnInit {

  isLoginMode = true;

  loginForm: FormGroup;
  registerForm: FormGroup;

  isLoading = false;
  errorMessage = '';
  successMessage = '';
  showLoginPassword = false;
  showRegisterPassword = false;

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

    this.registerForm = this.fb.group({
      fullName: ['', Validators.required],
      username: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      dateOfBirth: ['', Validators.required],
      gender: ['', Validators.required],
      contactInfo: ['', Validators.required],
      address: ['', Validators.required]
    });
  }

  toggleVisibility(field: 'login' | 'register') {
    if (field === 'login') this.showLoginPassword = !this.showLoginPassword;
    if (field === 'register') this.showRegisterPassword = !this.showRegisterPassword;
  }

  // Wipes stale tokens every time the login page loads
  ngOnInit() {
    localStorage.removeItem('token');
    localStorage.removeItem('currentUser');
    localStorage.removeItem('jwt');
  }

  get isCitizenSelected(): boolean {
    return this.loginForm.get('userType')?.value === 'Citizen';
  }

  toggleMode() {
    this.isLoginMode = !this.isLoginMode;
    this.errorMessage = '';
    this.successMessage = '';
    this.loginForm.reset({ userType: 'Citizen' });
    this.registerForm.reset({ gender: '' });
  }

  // --- LOGIN LOGIC ---
  onSubmit() {
    if (this.loginForm.invalid) return;

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';
    this.cdr.detectChanges();

    this.authService.login(this.loginForm.value).subscribe({
      next: (res: any) => { // Changed strictly for flexibility with the C# response casing
        this.isLoading = false;

        // 1. Save the token
        const validToken = res.token || res.Token;
        if (validToken) {
          localStorage.setItem('token', validToken);
        }

        // 2. Safely grab the role and name regardless of how the C# API capitalized it
        const userRole = res.role || res.Role || this.loginForm.value.userType;
        const userName = res.fullName || res.FullName || res.username || res.Username || this.loginForm.value.username;

        localStorage.setItem('userRole', userRole);
        localStorage.setItem('userName', userName);

        this.cdr.detectChanges();

        // 3. The Route Dictionary: Map every role to its specific dashboard
        const roleRedirects: { [key: string]: string } = {
          'Admin': '/admin-dashboard',
          'Citizen': '/citizen-dashboard',
          'ProgramManager': '/program-manager/dashboard',
          'WelfareOfficer': '/officer-dashboard',
          'ComplianceOfficer': '/compliance-dashboard',
          'GovernmentAuditor': '/auditor-dashboard'
        };

        // 4. Look up the route. If the role is missing/invalid, fall back to standard dashboard.
        const targetRoute = roleRedirects[userRole] || '/dashboard';

        // 5. Navigate!
        this.router.navigate([targetRoute]);
      },
      error: (err: HttpErrorResponse) => this.handleError(err)
    });
  }

  // --- REGISTRATION LOGIC ---
  onRegisterSubmit() {
    if (this.registerForm.invalid) return;

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';
    this.cdr.detectChanges();

    const newCitizen: RegisterCitizenRequest = {
      username: this.registerForm.value.username,
      password: this.registerForm.value.password,
      email: this.registerForm.value.email,
      fullName: this.registerForm.value.fullName,
      dateOfBirth: this.registerForm.value.dateOfBirth,
      gender: this.registerForm.value.gender,
      contactInfo: this.registerForm.value.contactInfo,
      address: this.registerForm.value.address
    };

    this.authService.registerCitizen(newCitizen).subscribe({
      next: () => {
        this.isLoading = false;
        this.successMessage = 'Registration successful! Please log in.';
        this.isLoginMode = true;
        this.registerForm.reset({ gender: '' });
        this.cdr.detectChanges();
      },
      error: (err: HttpErrorResponse) => this.handleError(err)
    });
  }

  // --- SHARED ERROR HANDLING ---
  private handleError(err: HttpErrorResponse) {
    this.isLoading = false;
    const errData = err.error as AuthErrorResponse;

    if (errData && errData.error) {
      this.errorMessage = errData.error;
    } else if (errData && errData.errors) {
      const firstErrorKey = Object.keys(errData.errors)[0];
      this.errorMessage = errData.errors[firstErrorKey][0];
    } else if (errData && errData.Error) {
      this.errorMessage = errData.Error;
    } else {
      this.errorMessage = 'A network or server error occurred. Please try again.';
    }

    this.cdr.detectChanges();
  }
}