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
  
  currentUserId!: number;
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
      this.currentUserId = Number(payload.UserId || payload.sub || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier']);
      this.loadData();
    }
  }

  loadData() {
    this.citizenService.getPrograms().subscribe(progs => {
      this.program = progs.find(p => p.programID === this.programId) || null;
    });

    this.citizenService.getDocuments(this.currentUserId).subscribe({
      next: (docs) => {
        // We only want to let them submit Approved or Pending docs!
        this.documents = docs.filter(d => d.verificationStatus !== 'Rejected');
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load your documents.';
        this.isLoading = false;
      }
    });
  }

  // --- Checkbox Logic ---
  toggleDoc(docId: number) {
    if (this.selectedDocIds.has(docId)) {
      this.selectedDocIds.delete(docId);
    } else {
      this.selectedDocIds.add(docId);
    }
  }

  // --- Validation Logic ---
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
    // If no docs required, or if all required docs are met, and they selected at least something if requirements exist
    if (this.requiredDocTypes.length === 0) return true;
    return this.missingRequirements.length === 0;
  }

  onSubmit() {
    if (!this.isFormValid || !this.program) return;
    this.isSubmitting = true;

    const payload: ApplyProgramRequest = {
      citizenID: this.currentUserId,
      programID: this.program.programID,
      selectedDocumentIds: Array.from(this.selectedDocIds)
    };

    this.citizenService.applyForProgram(payload).subscribe({
      next: () => {
        // Route them to their applications list upon success
        this.router.navigate(['/citizen/my-applications']);
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Failed to submit application.';
        this.isSubmitting = false;
      }
    });
  }
}