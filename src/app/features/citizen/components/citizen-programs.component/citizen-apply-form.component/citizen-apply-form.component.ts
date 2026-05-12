import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CitizenNavbarComponent } from '../../citizen-navbar.component/citizen-navbar.component';
import { CitizenService } from '../../../services/citizen.service';
import { WelfareProgram, CitizenDocument, ApplyProgramRequest } from '../../../models/citizen.model';

@Component({
  selector: 'app-citizen-apply-form',
  standalone: true,
  imports: [CommonModule, RouterModule, CitizenNavbarComponent],
  templateUrl: './citizen-apply-form.component.html'
})
export class CitizenApplyFormComponent implements OnInit {
  programId!: number;
  program: WelfareProgram | null = null;
  documents: CitizenDocument[] = [];
  selectedDocIds = new Set<number>();
  
  tokenUserId!: number;
  actualCitizenId!: number; // Track true ID

  isLoading = true;
  isSubmitting = false;
  errorMessage = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private citizenService: CitizenService
  ) {}

  ngOnInit(): void {
    this.programId = Number(this.route.snapshot.paramMap.get('id'));
    const token = localStorage.getItem('token');
    
    if (token) {
      const payload = JSON.parse(atob(token.split('.')[1]));
      this.tokenUserId = Number(payload.UserId || payload.sub || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier']);
      
      // 1. Fetch Profile first to get the true CitizenId!
      this.citizenService.getProfile(this.tokenUserId).subscribe({
        next: (profile) => {
          this.actualCitizenId = profile.citizenId ?? 0;
          this.loadData(); // 2. Now load documents and programs
        },
        error: () => {
          this.errorMessage = "Failed to load profile. Cannot apply.";
          this.isLoading = false;
        }
      });
    }
  }

  loadData() {
    this.citizenService.getPrograms().subscribe(progs => {
      const p = progs.find(p => p.programID === this.programId) || null;
      if (p) {
         const start = new Date(p.startDate).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
         const end = new Date(p.endDate).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
         
         p.duration = `${start} - ${end}`;
         p.requiredDocuments = p.requiredDocuments || (p as any).RequiredDocuments || 'None';
      }
      this.program = p;
    });

    // We now use actualCitizenId here to find your uploaded docs!
    this.citizenService.getDocuments(this.actualCitizenId).subscribe({
      next: (docs) => {
        this.documents = docs.filter(d => d.verificationStatus !== 'Rejected');
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load your documents.';
        this.isLoading = false;
      }
    });
  }

  toggleDoc(docId: number) {
    if (this.selectedDocIds.has(docId)) {
      this.selectedDocIds.delete(docId);
    } else {
      this.selectedDocIds.add(docId);
    }
  }

  get requiredDocTypes(): string[] {
    if (!this.program || !this.program.requiredDocuments || this.program.requiredDocuments === 'None') return [];
    return this.program.requiredDocuments.split(',').map(s => s.trim());
  }

  isRequirementMet(reqType: string): boolean {
    const selectedDocs = this.documents.filter(d => this.selectedDocIds.has(d.documentID));
    return selectedDocs.some(d => d.docType === reqType);
  }

  get missingRequirements(): string[] {
    return this.requiredDocTypes.filter(req => !this.isRequirementMet(req));
  }

  get isFormValid(): boolean {
    if (this.requiredDocTypes.length === 0) return true;
    return this.missingRequirements.length === 0;
  }

  onSubmit() {
    if (!this.isFormValid || !this.program || !this.actualCitizenId) return;
    this.isSubmitting = true;

    const payload: ApplyProgramRequest = {
      citizenID: this.actualCitizenId, // Ensure true ID is sent to backend
      programID: this.program.programID,
      selectedDocumentIds: Array.from(this.selectedDocIds)
    };

    this.citizenService.applyForProgram(payload).subscribe({
      next: () => {
        this.router.navigate(['/citizen/my-applications']);
      },
      error: (err) => {
        this.errorMessage = err.error?.message || err.error?.Error || 'Failed to submit application.';
        this.isSubmitting = false;
      }
    });
  }
}