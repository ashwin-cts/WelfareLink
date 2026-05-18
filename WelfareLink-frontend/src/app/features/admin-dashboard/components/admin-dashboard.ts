import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';

// Import Admin Navbar
import { AdminNavbarComponent } from './admin-navbar/admin-navbar';

import { AdminService } from '../services/admin.service';
import { UserProfile, SystemLog } from '../models/admin.model';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, AdminNavbarComponent],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.css'
})
export class AdminDashboard implements OnInit {
  activeTab: 'users' | 'logs' | 'create-officer' | 'create-admin' = 'users';

  usersList: UserProfile[] = [];
  logsList: SystemLog[] = [];
  
  currentAdminId: number | null = null;
  
  isLoading = false;
  isLogsLoading = false;
  currentPage = 1;
  totalPages = 1;

  officerForm: FormGroup;
  adminForm: FormGroup;
  
  formSuccessMessage = '';
  formErrorMessage = '';
  isFormLoading = false;
  
  constructor(
    private adminService: AdminService,
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
  }

  ngOnInit() {
    this.extractUserIdFromToken();
    this.loadUsers();
  }

  extractUserIdFromToken() {
    const token = localStorage.getItem('token') || localStorage.getItem('jwt'); 
    
    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        const nameIdentifier = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
        
        this.currentAdminId = Number(payload.UserId || payload.sub || nameIdentifier);
      } catch (e) {
        console.error("Could not decode token", e);
      }
    }
  }

  switchTab(tabName: 'users' | 'logs' | 'create-officer' | 'create-admin') {
    this.activeTab = tabName;
    this.clearMessages();
    
    if (tabName === 'users') this.loadUsers();
    if (tabName === 'logs') this.loadLogs(1);
  }

  loadUsers() {
    this.isLoading = true;
    this.adminService.getUsers().subscribe({
      next: (data) => { this.usersList = data; this.isLoading = false; this.cdr.detectChanges(); },
      error: (err) => { console.error(err); this.isLoading = false; this.cdr.detectChanges(); }
    });
  }

  toggleUserStatus(user: UserProfile) {
    if (!user.userId) return; 
    
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
      error: (err) => this.handleFormError(err)
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
      error: (err) => this.handleFormError(err)
    });
  }

  private handleFormError(err: any) {
    this.isFormLoading = false;
    
    let errorText = 'An error occurred. Please try again.';
    if (err.error && err.error.Error) errorText = err.error.Error; 
    else if (err.error && err.error.error) errorText = err.error.error; 
    else if (err.error && err.error.errors) {
      const firstErrorKey = Object.keys(err.error.errors)[0];
      errorText = err.error.errors[firstErrorKey][0]; 
    }

    this.formErrorMessage = errorText;
    this.cdr.detectChanges();
  }

  private clearMessages() {
    this.formSuccessMessage = '';
    this.formErrorMessage = '';
    this.cdr.detectChanges();
  }
}