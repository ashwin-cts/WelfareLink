import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ComplianceOfficerService } from '../../services/compliance-officer';
import { ComplianceFinding, ComplianceMetrics, UserProfile, SystemLog, CreateUserRequest, ComplianceRecord, ComplianceRecordCreateRequest, AuditEntry, AuditCreateRequest, NotificationItem, DashboardApplication } from '../../models/compliance-officer';

// Import the new components
import { ChangePasswordComponent } from '../change-password.component/change-password.component';
import { EditProfileComponent } from '../edit-profile.component/edit-profile.component';

@Component({
  selector: 'app-compliance-officer',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ChangePasswordComponent, EditProfileComponent],
  templateUrl: './compliance-officer.html',
  styleUrls: ['./compliance-officer.css'],
})
export class ComplianceOfficer implements OnInit {
  activeTab: 'dashboard' | 'applications' | 'allocations' | 'profile' | 'change-password' | 'users' | 'logs' | 'create-officer' | 'create-admin' | 'records' | 'audits' | 'notifications' = 'dashboard';
  tabDropdownOpen = false;
  accountDropdownOpen = false;

  isLoading = true;
  summary: ComplianceMetrics = {
    total: 0,
    open: 0,
    resolved: 0,
    issuesByType: []
  };
  findings: ComplianceFinding[] = [];
  activeFilter: 'open' | 'resolved' | 'recent' = 'open';
  dashboardApplications: DashboardApplication[] = [];
  message = '';

  // For applications tab
  allApplications: any[] = [];
  isApplicationsLoading = false;

  // For allocations tab
  allAllocations: any[] = [];
  isAllocationsLoading = false;

  // For users tab
  usersList: UserProfile[] = [];
  isUsersLoading = false;

  // For logs tab
  logsList: SystemLog[] = [];
  isLogsLoading = false;
  currentPage = 1;
  totalPages = 1;

  // For create forms
  officerForm: FormGroup;
  adminForm: FormGroup;
  formSuccessMessage = '';
  formErrorMessage = '';
  isFormLoading = false;

  // Compliance record and audit state
  complianceRecords: ComplianceRecord[] = [];
  recordForm: FormGroup;
  auditList: AuditEntry[] = [];
  auditForm: FormGroup;
  notifications: NotificationItem[] = [];
  isNotificationsLoading = false;
  isRecordsLoading = false;
  isAuditsLoading = false;
  newAuditStatusOptions = ['Pending', 'Completed', 'In Review'];

  constructor(
    private complianceService: ComplianceOfficerService,
    private router: Router,
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

    this.recordForm = this.fb.group({
      entityId: [null, Validators.required],
      entityType: ['Application', Validators.required],
      result: ['', Validators.required],
      notes: ['']
    });

    this.auditForm = this.fb.group({
      scope: ['', Validators.required],
      findings: ['', Validators.required],
      status: ['Pending', Validators.required]
    });
  }

  ngOnInit() {
    this.loadDashboard();
  }

  loadDashboard() {
    this.isLoading = true;
    this.message = '';

    this.complianceService.getMetrics().subscribe({
      next: (summary: ComplianceMetrics) => {
        this.summary = summary;
      },
      error: () => {
        this.message = 'Unable to load dashboard summary. Please try again later.';
      }
    });

    this.complianceService.getIssues().subscribe({
      next: (findings: ComplianceFinding[]) => {
        this.findings = findings;
        this.isLoading = false;
      },
      error: () => {
        this.message = 'Unable to load compliance findings. Please refresh.';
        this.isLoading = false;
      }
    });
    this.loadDashboardApplications();
  }

  loadDashboardApplications() {
    this.complianceService.getDashboardApplications().subscribe({
      next: (applications) => {
        this.dashboardApplications = applications;
      },
      error: (err) => {
        console.error('Dashboard applications error', err);
      }
    });
  }

  loadApplications() {
    this.isApplicationsLoading = true;
    this.complianceService.getAllApplicationsWithBenefits().subscribe({
      next: (applications) => {
        this.allApplications = applications;
        this.isApplicationsLoading = false;
      },
      error: (err) => {
        console.error('Error loading applications', err);
        this.message = 'Unable to load applications. Please try again later.';
        this.isApplicationsLoading = false;
      }
    });
  }

  loadAllocations() {
    this.isAllocationsLoading = true;
    this.complianceService.getBenefitAllocations().subscribe({
      next: (allocations) => {
        this.allAllocations = allocations;
        this.isAllocationsLoading = false;
      },
      error: (err) => {
        console.error('Error loading allocations', err);
        this.message = 'Unable to load allocations. Please try again later.';
        this.isAllocationsLoading = false;
      }
    });
  }

  switchTab(tabName: 'dashboard' | 'applications' | 'allocations' | 'profile' | 'change-password' | 'users' | 'logs' | 'create-officer' | 'create-admin' | 'records' | 'audits' | 'notifications') {
    this.activeTab = tabName;
    this.clearMessages();
    this.tabDropdownOpen = false;
    this.accountDropdownOpen = false;
    
    if (tabName === 'applications') this.loadApplications();
    if (tabName === 'allocations') this.loadAllocations();
    if (tabName === 'users') this.loadUsers();
    if (tabName === 'logs') this.loadLogs(1);
    if (tabName === 'records') this.loadComplianceRecords();
    if (tabName === 'audits') this.loadAudits();
    if (tabName === 'notifications') this.loadNotifications();
  }

  toggleTabDropdown() {
    this.tabDropdownOpen = !this.tabDropdownOpen;
    if (this.tabDropdownOpen) {
      this.accountDropdownOpen = false;
    }
  }

  toggleAccountDropdown() {
    this.accountDropdownOpen = !this.accountDropdownOpen;
    if (this.accountDropdownOpen) {
      this.tabDropdownOpen = false;
    }
  }

  selectTab(tabName: 'dashboard' | 'applications' | 'allocations' | 'profile' | 'change-password' | 'users' | 'logs' | 'create-officer' | 'create-admin' | 'records' | 'audits' | 'notifications') {
    this.switchTab(tabName);
  }

  getTabLabel(tabName: 'dashboard' | 'applications' | 'allocations' | 'profile' | 'change-password' | 'users' | 'logs' | 'create-officer' | 'create-admin' | 'records' | 'audits' | 'notifications') {
    switch (tabName) {
      case 'dashboard': return 'Dashboard';
      case 'applications': return 'Applications';
      case 'allocations': return 'Allocations';
      case 'profile': return 'Profile';
      case 'change-password': return 'Change Password';
      case 'users': return 'Users';
      case 'logs': return 'Logs';
      case 'create-officer': return 'Create Officer';
      case 'create-admin': return 'Create Admin';
      case 'records': return 'Compliance Records';
      case 'audits': return 'Audits';
      case 'notifications': return 'Notifications';
      default: return 'Dashboard';
    }
  }

  loadUsers() {
    this.isUsersLoading = true;
    this.complianceService.getUsers().subscribe({
      next: (data) => { this.usersList = data; this.isUsersLoading = false; },
      error: (err) => { console.error(err); this.isUsersLoading = false; }
    });
  }

  toggleUserStatus(user: UserProfile) {
    if (!user.userId) return; 
    
    if (user.isActive) {
      this.complianceService.blockUser(user.userId).subscribe(() => { user.isActive = false; });
    } else {
      this.complianceService.unblockUser(user.userId).subscribe(() => { user.isActive = true; });
    }
  }

  loadLogs(pageNumber: number) {
    if (pageNumber < 1 || (this.totalPages > 0 && pageNumber > this.totalPages)) return;
    this.isLogsLoading = true;
    this.complianceService.getSystemLogs(pageNumber, 10).subscribe({
      next: (res) => {
        this.logsList = res.items || res.data || res.records || (Array.isArray(res) ? res : []); 
        this.currentPage = res.pageNumber || res.currentPage || 1; 
        this.totalPages = res.totalPages || 1;
        this.isLogsLoading = false; 
      },
      error: (err) => { 
        console.error("Logs Error:", err); 
        this.isLogsLoading = false; 
      }
    });
  }

  loadComplianceRecords() {
    this.isRecordsLoading = true;
    this.complianceService.getComplianceRecords().subscribe({
      next: (data) => {
        this.complianceRecords = data;
        this.isRecordsLoading = false;
      },
      error: (err) => {
        console.error('Compliance records error', err);
        this.isRecordsLoading = false;
      }
    });
  }

  submitComplianceRecord() {
    if (this.recordForm.invalid) return;
    this.isFormLoading = true;
    this.clearMessages();

    const payload: ComplianceRecordCreateRequest = this.recordForm.value;
    this.complianceService.createComplianceRecord(payload).subscribe({
      next: () => {
        this.formSuccessMessage = 'Compliance record created successfully!';
        this.recordForm.reset({ entityType: 'Application', notes: '' });
        this.isFormLoading = false;
        this.loadComplianceRecords();
      },
      error: (err) => this.handleFormError(err)
    });
  }

  resolveIssue(finding: ComplianceFinding) {
    const notes = prompt('Enter resolution notes for this issue:');
    if (!notes) return;
    this.complianceService.resolveIssue(finding.recordID, notes).subscribe({
      next: () => {
        finding.status = 'Resolved';
        finding.resolvedDate = new Date().toISOString();
      },
      error: (err) => {
        console.error('Resolve issue error', err);
      }
    });
  }

  loadAudits() {
    this.isAuditsLoading = true;
    this.complianceService.getAudits().subscribe({
      next: (data) => {
        this.auditList = data;
        this.isAuditsLoading = false;
      },
      error: (err) => {
        console.error('Audit loading error', err);
        this.isAuditsLoading = false;
      }
    });
  }

  submitAudit() {
    if (this.auditForm.invalid) return;
    this.isFormLoading = true;
    this.clearMessages();

    const payload: AuditCreateRequest = this.auditForm.value;
    this.complianceService.createAudit(payload).subscribe({
      next: () => {
        this.formSuccessMessage = 'Audit saved successfully!';
        this.auditForm.reset({ status: 'Pending' });
        this.isFormLoading = false;
        this.loadAudits();
      },
      error: (err) => this.handleFormError(err)
    });
  }

  loadNotifications() {
    this.isNotificationsLoading = true;
    this.complianceService.getNotifications().subscribe({
      next: (data) => {
        this.notifications = data;
        this.isNotificationsLoading = false;
      },
      error: (err) => {
        console.error('Notifications error', err);
        this.isNotificationsLoading = false;
      }
    });
  }

  markAsRead(notification: NotificationItem) {
    this.complianceService.markNotificationRead(notification.notificationId).subscribe({
      next: () => {
        notification.status = 'Read';
      },
      error: (err) => {
        console.error('Notification read error', err);
      }
    });
  }

  submitOfficer() {
    if (this.officerForm.invalid) return;
    this.isFormLoading = true;
    this.clearMessages();

    this.complianceService.createOfficer(this.officerForm.value).subscribe({
      next: () => {
        this.isFormLoading = false;
        this.formSuccessMessage = 'Officer created successfully!';
        this.officerForm.reset({ role: '' }); 
      },
      error: (err) => this.handleFormError(err)
    });
  }

  submitAdmin() {
    if (this.adminForm.invalid) return;
    this.isFormLoading = true;
    this.clearMessages();

    this.complianceService.createAdmin(this.adminForm.value).subscribe({
      next: () => {
        this.isFormLoading = false;
        this.formSuccessMessage = 'System Admin created successfully!';
        this.adminForm.reset({ role: 'Admin' });
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
  }

  private clearMessages() {
    this.formSuccessMessage = '';
    this.formErrorMessage = '';
  }

  get filteredFindings() {
    if (this.activeFilter === 'resolved') {
      return this.findings.filter(item => item.status === 'Resolved');
    }
    if (this.activeFilter === 'recent') {
      return this.findings.slice(0, 6);
    }
    return this.findings.filter(item => item.status === 'Open');
  }

  setFilter(filter: 'open' | 'resolved' | 'recent') {
    this.activeFilter = filter;
  }

  getStatusBadgeClass(status: string): string {
    switch(status?.toLowerCase()) {
      case 'pending': return 'bg-warning';
      case 'approved': return 'bg-success';
      case 'rejected': return 'bg-danger';
      case 'submitted': return 'bg-info';
      default: return 'bg-secondary';
    }
  }

  viewApplicationDetails(applicationId: number) {
    this.complianceService.getApplicationDetails(applicationId).subscribe({
      next: (details) => {
        console.log('Application Details:', details);
        // You can add a modal or navigate to a details page
      },
      error: (err) => {
        console.error('Error loading application details', err);
        this.message = 'Unable to load application details.';
      }
    });
  }

  viewAllocationDetails(benefitId: number) {
    console.log('Viewing allocation details for benefit ID:', benefitId);
    // You can add a modal or navigate to a details page
  }

  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('jwt');
    this.router.navigate(['/login']);
  }
}
