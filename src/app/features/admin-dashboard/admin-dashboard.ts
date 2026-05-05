import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { AdminService } from '../../core/services/admin/admin.service';
import { AuthService } from '../../core/services/auth/auth.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.css'
})
export class AdminDashboard implements OnInit {
  activeTab: 'users' | 'logs' | 'create-officer' | 'create-admin' | 'profile' = 'users';

  usersList: any[] = [];
  logsList: any[] = [];
  currentAdminId: number | null = null;
  
  isLoading = false;
  isLogsLoading = false;
  currentPage = 1;
  totalPages = 1;

  officerForm: FormGroup;
  adminForm: FormGroup;
  profileForm: FormGroup;
  passwordForm: FormGroup; 
  
  formSuccessMessage = '';
  formErrorMessage = '';
  pwdSuccessMessage = '';
  pwdErrorMessage = '';
  isFormLoading = false;
  isPwdLoading = false;
  
  currentProfile: any = null;

  constructor(
    private adminService: AdminService,
    private authService: AuthService,
    private router: Router,
    private cdr: ChangeDetectorRef,
    private fb: FormBuilder 
  ) {
    this.officerForm = this.fb.group({
      username: ['', Validators.required],
      password: ['', Validators.required],
      role: ['', Validators.required],
      fullName: [''],
      email: ['']
    });

    this.adminForm = this.fb.group({
      username: ['', Validators.required],
      password: ['', Validators.required],
      role: ['Admin', Validators.required],
      fullName: [''],
      email: ['']
    });

    this.profileForm = this.fb.group({
      fullName: [''],
      email: ['']
    });

    this.passwordForm = this.fb.group({
      currentPassword: ['', Validators.required],
      newPassword: ['', [Validators.required, Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9\s])\S{8,}$/)]],
      confirmPassword: ['', Validators.required]
    });
  }

  ngOnInit() {
    this.extractUserIdFromToken();
    this.loadUsers();
  }

  extractUserIdFromToken() {
    // 1. Check if the token is saved under a different name (adjust if your auth service uses a different key!)
    const token = localStorage.getItem('token') || localStorage.getItem('jwt'); 
    
    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        console.log("JWT Payload Decoded:", payload); // <-- This will show you exactly what C# sent!

        // ASP.NET sometimes maps claims to full schemas, we check all possible variations
        const nameIdentifier = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
        
        // Force the ID to be a Number
        this.currentAdminId = Number(payload.UserId || payload.sub || nameIdentifier);
        console.log("Extracted Admin ID:", this.currentAdminId);

      } catch (e) {
        console.error("Could not decode token", e);
      }
    } else {
      console.warn("No token found in localStorage!");
    }
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  switchTab(tabName: 'users' | 'logs' | 'create-officer' | 'create-admin' | 'profile') {
    this.activeTab = tabName;
    this.clearMessages();
    
    if (tabName === 'users') this.loadUsers();
    if (tabName === 'logs') this.loadLogs(1);
    if (tabName === 'profile') this.loadProfile();
  }

  loadUsers() {
    this.isLoading = true;
    this.adminService.getUsers().subscribe({
      next: (data) => { this.usersList = data; this.isLoading = false; this.cdr.detectChanges(); },
      error: (err) => { console.error(err); this.isLoading = false; this.cdr.detectChanges(); }
    });
  }

  toggleUserStatus(user: any) {
    if (user.isActive) {
      this.adminService.blockUser(user.userId).subscribe(() => { user.isActive = false; this.cdr.detectChanges(); });
    } else {
      this.adminService.unblockUser(user.userId).subscribe(() => { user.isActive = true; this.cdr.detectChanges(); });
    }
  }

  loadLogs(pageNumber: number) {
    if (pageNumber < 1 || (this.totalPages > 0 && pageNumber > this.totalPages)) return;
    this.isLogsLoading = true;
    this.adminService.getSystemLogs(pageNumber, 10).subscribe({
      next: (res) => {
        // --- MAGIC FIX #2: CATCH ALL PAGINATION JSON FORMATS ---
        this.logsList = res.items || res.data || res.records || (Array.isArray(res) ? res : []); 
        this.currentPage = res.pageNumber || res.currentPage || 1; 
        this.totalPages = res.totalPages || 1;
        this.isLogsLoading = false; 
        this.cdr.detectChanges();
      },
      error: (err) => { 
        console.error("Logs Error:", err); 
        this.isLogsLoading = false; 
        this.cdr.detectChanges(); 
      }
    });
  }

  loadProfile() {
    // 2. If the ID is missing, stop the spinner and show an error!
    if (!this.currentAdminId) {
      console.error("Cannot load profile: currentAdminId is null!");
      this.currentProfile = { username: 'Unknown User' }; // Fakes a profile to stop the spinner
      this.formErrorMessage = "Session error: Could not extract User ID. Please try logging out and back in.";
      this.cdr.detectChanges();
      return;
    }

    console.log(`Sending request to UserApi for ID: ${this.currentAdminId}`);

    this.adminService.getProfile(this.currentAdminId).subscribe({
      next: (data) => {
        console.log("Profile Data Received:", data);
        this.currentProfile = data;
        this.profileForm.patchValue({
          fullName: data.fullName,
          email: data.email
        });
        this.cdr.detectChanges();
      },
      error: (err) => {
        // 3. If the C# API fails, stop the spinner and show the error!
        console.error("Profile API Error:", err);
        this.currentProfile = { username: 'Error Loading Profile' }; // Stop the spinner
        this.formErrorMessage = "Failed to load profile from the server. Check console for details.";
        this.cdr.detectChanges();
      }
    });
  }

  updateProfile() {
    if (this.profileForm.invalid || !this.currentAdminId) return;
    this.isFormLoading = true;
    this.clearMessages();
    
    this.adminService.updateProfile(this.currentAdminId, this.profileForm.value).subscribe({
      next: (res) => {
        this.isFormLoading = false;
        this.formSuccessMessage = 'Profile updated successfully!';
        this.currentProfile = res; 
        this.cdr.detectChanges();
      },
      error: (err) => this.handleFormError(err, 'profile')
    });
  }

  submitPasswordChange() {
    if (this.passwordForm.invalid || !this.currentAdminId) return;

    const vals = this.passwordForm.value;
    if (vals.newPassword !== vals.confirmPassword) {
      this.pwdErrorMessage = "New passwords do not match!";
      return;

    }
      //currentPassword: ['', Validators.required],
    else if(vals.currentPassword==vals.newPassword){
      this.pwdErrorMessage = "You cannot use the old password!";
      return;
    }

    this.isPwdLoading = true;
    this.clearMessages();

    const payload = {
      currentPassword: vals.currentPassword,
      newPassword: vals.newPassword
    };

    this.adminService.changePassword(this.currentAdminId, payload).subscribe({
      next: () => {
        this.isPwdLoading = false;
        this.pwdSuccessMessage = 'Password changed successfully!';
        this.passwordForm.reset();
        this.cdr.detectChanges();
      },
      error: (err) => this.handleFormError(err, 'password')
    });
  }

  submitOfficer() {
    if (this.officerForm.invalid) return;
    this.isFormLoading = true;
    this.clearMessages();

    this.adminService.createOfficer(this.officerForm.value).subscribe({
      next: () => {
        this.isFormLoading = false;
        this.formSuccessMessage = 'Officer created successfully!';
        this.officerForm.reset({ role: '' }); 
        this.cdr.detectChanges();
      },
      error: (err) => this.handleFormError(err, 'profile')
    });
  }

  submitAdmin() {
    if (this.adminForm.invalid) return;
    this.isFormLoading = true;
    this.clearMessages();

    this.adminService.createAdmin(this.adminForm.value).subscribe({
      next: () => {
        this.isFormLoading = false;
        this.formSuccessMessage = 'System Admin created successfully!';
        this.adminForm.reset({ role: 'Admin' });
        this.cdr.detectChanges();
      },
      error: (err) => this.handleFormError(err, 'profile')
    });
  }

  private handleFormError(err: any, formType: 'profile' | 'password') {
    this.isFormLoading = false;
    this.isPwdLoading = false;
    
    let errorText = 'An error occurred. Please try again.';
    if (err.error && err.error.Error) errorText = err.error.Error; 
    else if (err.error && err.error.error) errorText = err.error.error; 
    else if (err.error && err.error.errors) {
      const firstErrorKey = Object.keys(err.error.errors)[0];
      errorText = err.error.errors[firstErrorKey][0]; 
    }

    if (formType === 'password') this.pwdErrorMessage = errorText;
    else this.formErrorMessage = errorText;
    
    this.cdr.detectChanges();
  }

  private clearMessages() {
    this.formSuccessMessage = '';
    this.formErrorMessage = '';
    this.pwdSuccessMessage = '';
    this.pwdErrorMessage = '';
    this.cdr.detectChanges();
  }
}