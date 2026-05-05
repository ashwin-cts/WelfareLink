import { Component, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth/auth.service';

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
      next: (res) => {
        this.isLoading = false;
        this.cdr.detectChanges();
        
        // --- MAGIC FIX: Save the token to the browser's memory! ---
        // (We use res.token || res.Token just in case C# capitalized it)
        if (res.token || res.Token) {
          localStorage.setItem('token', res.token || res.Token);
        }
        
        if (res.role === 'Admin' || res.Role === 'Admin') {
          this.router.navigate(['/admin-dashboard']);
        } else {
          this.router.navigate(['/dashboard']); 
        }
      },
      error: (err) => {
        this.isLoading = false;
        
        // 1. Check for manual C# business logic errors (e.g., "Account is inactive")
        if (err.error && err.error.error) {
          this.errorMessage = err.error.error; 
        } 
        // 2. NEW: Check for C# Model Data Annotation errors (e.g., Password Regex fails)
        else if (err.error && err.error.errors) {
          // Grabs the first validation error from the dictionary and displays it
          const firstErrorKey = Object.keys(err.error.errors)[0];
          this.errorMessage = err.error.errors[firstErrorKey][0];
        } 
        // Catches your custom capitalized "Error" from UserManagement Program.cs!
       else if (err.error && err.error.Error) {
             this.errorMessage = err.error.Error;
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