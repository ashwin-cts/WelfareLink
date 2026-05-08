import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CitizenService } from '../../../services/citizen.service';
import { AuthService } from '../../../../auth/login/services/auth.service';
import { CitizenDocument, WelfareProgram, WelfareApplication } from '../../../models/citizen.model';

@Component({
  selector: 'app-citizen-dashboard',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './citizen-dashboard.html',
  styleUrl: './citizen-dashboard.css'
})
export class CitizenDashboard implements OnInit {
  // NEW: Added 'apply' to the allowed tabs
  activeTab: 'overview' | 'upload' | 'status' | 'programs' | 'applications' | 'profile' | 'apply' = 'overview';
  
  currentUserId: number | null = null;
  citizenData: any = null; 
  
  stats = { pending: 0, approved: 0, rejected: 0 };
  documents: CitizenDocument[] = [];
  programs: WelfareProgram[] = [];
  applications: WelfareApplication[] = [];

  // NEW: Application Form State Variables
  selectedProgramToApply: WelfareProgram | null = null;
  requiredDocTypes: string[] = [];
  selectedDocIdsForApp: number[] = [];

  profileForm: FormGroup;
  passwordForm: FormGroup;
  documentForm: FormGroup;
  selectedFile: File | null = null;
  showRemarks = false;
  
  isDropdownOpen = false;
  isLoading = false;
  message = { text: '', isError: false };

  constructor(
    private citizenService: CitizenService,
    private authService: AuthService,
    private fb: FormBuilder,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {
    this.profileForm = this.fb.group({
      name: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      contactInfo: ['', Validators.required],
      address: ['', Validators.required],
      dateOfBirth: ['', Validators.required],
      gender: ['', Validators.required]
    });

    this.passwordForm = this.fb.group({
      currentPassword: ['', Validators.required],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required]
    });

    this.documentForm = this.fb.group({
      documentType: ['', Validators.required],
      remarks: ['']
    });
  }

  ngOnInit() {
    this.extractUserId();
    if (this.currentUserId) {
      this.loadCitizenProfile();
    }
    this.loadPrograms(); 
  }

  extractUserId() {
    const token = localStorage.getItem('token') || localStorage.getItem('jwt');
    if (token) {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const nameIdentifier = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
      this.currentUserId = Number(payload.UserId || payload.sub || nameIdentifier);
    }
  }

  loadCitizenProfile() {
    if (!this.currentUserId) return;
    this.citizenService.getProfile(this.currentUserId).subscribe({
      next: (data) => {
        this.citizenData = data;
        if (data.dateOfBirth) data.dateOfBirth = data.dateOfBirth.split('T')[0]; 
        this.profileForm.patchValue(data);
        this.loadDashboardData();
      },
      error: (err) => console.error("Error loading profile", err)
    });
  }

  loadDashboardData() {
    if (!this.citizenData) return;
    const idToUse = this.citizenData.id || this.citizenData.citizenId;

    this.citizenService.getDashboardStats(idToUse).subscribe({
      next: (res) => {
        this.stats = { 
          pending: res.pendingDocuments || 0, 
          approved: res.approvedDocuments || 0, 
          rejected: res.rejectedDocuments || 0 
        };
        this.documents = res.documents || []; 
        this.loadApplications(); // Ensure applications are loaded to check button status
        this.cdr.detectChanges();
      },
      error: (err) => console.error("Dashboard Load Error:", err)
    });
  }

  switchTab(tab: any) {
    this.activeTab = tab;
    this.message = { text: '', isError: false };
    if (tab === 'programs') this.loadPrograms();
    if (tab === 'applications') this.loadApplications();
    if (tab === 'status') this.loadDashboardData(); 
  }

  toggleDropdown() {
    this.isDropdownOpen = !this.isDropdownOpen;
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  // --- DOCUMENT LOGIC ---
  onDocumentTypeChange(event: any) {
    this.showRemarks = event.target.value === 'Other';
  }

  onFileSelected(event: any) {
    this.selectedFile = event.target.files[0];
  }

  submitDocument() {
    const idToUse = this.citizenData?.id || this.citizenData?.citizenId;
    if (this.documentForm.invalid || !this.selectedFile || !idToUse) return;
    
    this.isLoading = true;
    const formData = new FormData();
    formData.append('file', this.selectedFile);
    formData.append('citizenId', idToUse.toString());
    formData.append('documentType', this.documentForm.value.documentType);
    if (this.showRemarks) formData.append('remarks', this.documentForm.value.remarks);

    this.citizenService.uploadDocument(formData).subscribe({
      next: () => {
        this.showMessage('Document uploaded successfully!', false);
        this.documentForm.reset();
        this.selectedFile = null;
        this.showRemarks = false;
        this.isLoading = false;
        this.loadDashboardData(); 
      },
      error: () => {
        this.showMessage('Failed to upload document.', true);
        this.isLoading = false;
      }
    });
  }

  deleteDocument(docId: number) {
    if (confirm('Are you sure you want to delete this document?')) {
      this.citizenService.deleteDocument(docId).subscribe({
        next: () => {
          this.showMessage('Document deleted successfully!', false);
          this.loadDashboardData(); 
        },
        error: () => this.showMessage('Failed to delete document.', true)
      });
    }
  }

  // --- APPLICATION & PROGRAM LOGIC ---
  loadPrograms() {
    this.citizenService.getPrograms().subscribe({
      next: (res) => {
        this.programs = res || [];
        this.cdr.detectChanges();
      },
      error: (err) => console.error("Failed to load programs", err)
    });
  }

  loadApplications() {
    const idToUse = this.citizenData?.id || this.citizenData?.citizenId;
    if (!idToUse) return;

    this.citizenService.getApplications(idToUse).subscribe({
      next: (res) => {
        this.applications = res || [];
        this.cdr.detectChanges();
      },
      error: (err) => console.error("Failed to load applications", err)
    });
  }

  // NEW: Checks if the user already applied to disable the button
  hasApplied(programId: number): boolean {
    return this.applications.some(app => app.programID === programId);
  }

  getProgramName(programId: number): string {
    const prog = this.programs.find(p => p.programID === programId);
    return prog ? prog.title : `Program #${programId}`;
  }

  // NEW: Opens the interactive application form view
  openApplyView(programId: number) {
    const prog = this.programs.find(p => p.programID === programId);
    if (!prog) return;

    this.selectedProgramToApply = prog;
    
    // Parse required documents from the C# string (e.g., "ID Proof, Residence Proof")
    const reqDocs = prog.requiredDocuments || 'None';
    this.requiredDocTypes = reqDocs.toLowerCase() === 'none' ? [] : reqDocs.split(',').map(s => s.trim());
    
    // Reset selections
    this.selectedDocIdsForApp = [];
    this.switchTab('apply');
  }

  // NEW: Toggles document selection checkboxes
  toggleDocumentSelection(docId: number, event: any) {
    if (event.target.checked) {
      this.selectedDocIdsForApp.push(docId);
    } else {
      this.selectedDocIdsForApp = this.selectedDocIdsForApp.filter(id => id !== docId);
    }
  }

  // NEW: Checks if a specific required document type has been selected
  isRequirementMet(docType: string): boolean {
    return this.selectedDocIdsForApp.some(id => {
      const doc = this.documents.find(d => d.documentID === id);
      // Case-insensitive comparison just in case
      return doc && doc.docType.toLowerCase() === docType.toLowerCase();
    });
  }

  // NEW: Determines if the final Submit button should be enabled
  canSubmitApplication(): boolean {
    if (this.requiredDocTypes.length === 0) return true; // No docs needed
    // Every required type must have at least one matching document selected
    return this.requiredDocTypes.every(type => this.isRequirementMet(type));
  }

  // REFACTORED: The actual API call now fires from the new Apply view
  submitFinalApplication() {
    const idToUse = this.citizenData?.id || this.citizenData?.citizenId;
    if (!idToUse || !this.selectedProgramToApply) return;

    this.isLoading = true;
    const payload = { 
      citizenID: idToUse, 
      programID: this.selectedProgramToApply.programID,
      selectedDocumentIds: this.selectedDocIdsForApp 
    };
    
    this.citizenService.applyForProgram(payload).subscribe({
      next: () => {
        this.showMessage('Successfully applied for program!', false);
        this.isLoading = false;
        this.loadApplications(); 
        this.switchTab('applications'); // Route them to view their new app
      },
      error: (err) => {
        const errorMsg = err.error?.Error || err.error?.error || 'Failed to apply. Check documents.';
        this.showMessage(errorMsg, true);
        this.isLoading = false;
      }
    });
  }

  // --- PROFILE LOGIC ---
  updateProfile() {
    const idToUse = this.citizenData?.id || this.citizenData?.citizenId;
    if (this.profileForm.invalid || !idToUse) return;

    this.citizenService.updateProfile(idToUse, this.profileForm.value).subscribe({
      next: () => this.showMessage('Profile updated!', false),
      error: () => this.showMessage('Update failed.', true)
    });
  }

  submitPasswordChange() {
    const idToUse = this.citizenData?.id || this.citizenData?.citizenId;
    if (this.passwordForm.invalid || !idToUse) return;

    const vals = this.passwordForm.value;
    if (vals.newPassword !== vals.confirmPassword) {
      this.showMessage("New passwords do not match!", true);
      return;
    }

    const payload = { currentPassword: vals.currentPassword, newPassword: vals.newPassword };

    this.citizenService.changePassword(idToUse, payload).subscribe({
      next: () => {
        this.showMessage('Password changed successfully!', false);
        this.passwordForm.reset();
      },
      error: (err) => {
        const errorMsg = err.error?.Error || 'Failed to change password.';
        this.showMessage(errorMsg, true);
      }
    });
  }

  showMessage(text: string, isError: boolean) {
    this.message = { text, isError };
    this.cdr.detectChanges();
  }
}