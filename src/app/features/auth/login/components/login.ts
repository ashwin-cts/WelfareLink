import { Component, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

// Make sure these paths match your folder structure!
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
    { id: 'GovernmentAuditor', label: 'Government Auditor' }
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
      // We expect the strict AuthResponse here
      next: (res: AuthResponse) => {
        this.isLoading = false;
        this.cdr.detectChanges();
        
        const validToken = res.token || res.Token;
        if (validToken) {
          localStorage.setItem('token', validToken);
        }
        
        if (res.role === 'Admin' || res.Role === 'Admin') {
          this.router.navigate(['/admin-dashboard']);
        } else {
          this.router.navigate(['/dashboard']); 
        }
      },
      
      // Use HttpErrorResponse to strict type the error instead of 'any'
      error: (err: HttpErrorResponse) => {
        this.isLoading = false;
        const errData = err.error as AuthErrorResponse; // Cast it to our strict error model
        
        // 1. Check for manual C# business logic errors 
        if (errData && errData.error) {
          this.errorMessage = errData.error; 
        } 
        // 2. NEW: Check for C# Model Data Annotation errors 
        else if (errData && errData.errors) {
          const firstErrorKey = Object.keys(errData.errors)[0];
          this.errorMessage = errData.errors[firstErrorKey][0];
        } 
        // Catches your custom capitalized "Error" from UserManagement Program.cs!
        else if (errData && errData.Error) {
          this.errorMessage = errData.Error;
        }
        // 3. Fallback for server crashes
        else {
          this.errorMessage = 'Invalid username or password. Please try again.';
        }
        
        this.cdr.detectChanges(); 
      }
    });
  }
}