import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { DisbursementService } from '../../services/disbursement.service';
import { BenefitDetails, Disbursement } from '../../models/disbursement.model';
import{ DisbursementNavbarComponent } from '../disbursement-navbar.component/disbursement-navbar.component';
@Component({
  selector: 'app-disbursement-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, DisbursementNavbarComponent],
  templateUrl: './disbursement-form.component.html',
  styleUrls: ['./disbursement-form.component.css']
})
export class DisbursementFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private disbursementService = inject(DisbursementService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  disbursementForm!: FormGroup;
  isEditMode = false;
  currentDisbursementId: number | null = null;
  
  // Data for the UI
  benefitDetails: BenefitDetails | null = null;
  isLoading = false;
  isSaving = false;
  errorMessage = '';

  ngOnInit(): void {
    this.initForm();

    this.route.paramMap.subscribe(params => {
      const idParam = params.get('id');
      if (idParam) {
        this.isEditMode = true;
        this.currentDisbursementId = Number(idParam);
        this.loadDisbursementData(this.currentDisbursementId);
        
        // Disable benefit selection in edit mode
        this.disbursementForm.get('benefitID')?.disable();
      } else {
        this.isEditMode = false;
        this.prefillOfficerId();
      }
    });
  }

  initForm(): void {
    this.disbursementForm = this.fb.group({
      benefitID: ['', Validators.required],
      amount: ['', [Validators.required, Validators.min(1)]],
      citizenID: ['', Validators.required],
      officerID: ['', Validators.required],
      date: [new Date().toISOString().split('T')[0], Validators.required],
      status: ['', Validators.required]
    });
  }

  // Simulates fetching officer ID from token
  prefillOfficerId(): void {
    const token = localStorage.getItem('token') || localStorage.getItem('jwt');
    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        const officerId = Number(payload.UserId || payload.sub || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier']);
        this.disbursementForm.patchValue({ officerID: officerId });
      } catch (e) {
        console.warn('Could not parse Officer ID');
      }
    }
  }

  // Called when the user types/changes the Benefit ID field
  onBenefitIdChange(): void {
    const benefitId = this.disbursementForm.get('benefitID')?.value;
    if (benefitId) {
      this.fetchBenefitDetails(benefitId);
    } else {
      this.benefitDetails = null;
      this.disbursementForm.patchValue({ citizenID: '' });
    }
  }

  fetchBenefitDetails(benefitId: number): void {
    this.isLoading = true;
    this.errorMessage = '';
    
    this.disbursementService.getBenefitDetails(benefitId).subscribe({
      next: (details) => {
        this.benefitDetails = details;
        // Auto-fill the citizen ID based on the benefit!
        if (details.citizenId) {
          this.disbursementForm.patchValue({ citizenID: details.citizenId });
        }
        this.isLoading = false;
      },
      error: () => {
        this.benefitDetails = null;
        this.errorMessage = `Could not find details for Benefit #${benefitId}`;
        this.isLoading = false;
      }
    });
  }

  loadDisbursementData(id: number): void {
    this.isLoading = true;
    this.disbursementService.getDisbursementById(id).subscribe({
      next: (data) => {
        // Format date for the HTML5 date input (YYYY-MM-DD)
        const formattedDate = data.date ? data.date.split('T')[0] : '';
        
        this.disbursementForm.patchValue({
          benefitID: data.benefitID,
          amount: data.amount,
          citizenID: data.citizenID,
          officerID: data.officerID,
          date: formattedDate,
          status: data.status
        });

        // Fetch the associated benefit details for the UI summary panel
        if (data.benefitID) {
          this.fetchBenefitDetails(data.benefitID);
        } else {
            this.isLoading = false;
        }
      },
      error: () => {
        this.errorMessage = 'Failed to load disbursement details.';
        this.isLoading = false;
      }
    });
  }

  onSubmit(): void {
    if (this.disbursementForm.invalid) {
      this.disbursementForm.markAllAsTouched();
      return;
    }

    // Prevent submission if resources are exhausted
    if (this.benefitDetails?.isResourceExhausted) {
      this.errorMessage = 'Cannot disburse. Programme resources are fully exhausted.';
      window.scrollTo({ top: 0, behavior: 'smooth' });
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';

    // If a control was disabled (like benefitID in Edit mode), it won't be in .value. 
    // We use .getRawValue() to get everything.
    const formData = this.disbursementForm.getRawValue();

    // Ensure the payload matches Swagger exactly
    const payload: Partial<Disbursement> = {
      benefitID: Number(formData.benefitID),
      citizenID: Number(formData.citizenID),
      officerID: Number(formData.officerID),
      amount: Number(formData.amount),
      date: formData.date,
      status: formData.status
    };

    if (this.isEditMode && this.currentDisbursementId) {
      payload.disbursementID = this.currentDisbursementId;
      
      this.disbursementService.updateDisbursement(this.currentDisbursementId, payload).subscribe({
        next: () => this.router.navigate(['/disbursements']),
        error: (err) => this.handleError(err)
      });
    } else {
      this.disbursementService.createDisbursement(payload).subscribe({
        next: () => this.router.navigate(['/disbursements']),
        error: (err) => this.handleError(err)
      });
    }
  }

  handleError(err: any): void {
    this.isSaving = false;
    this.errorMessage = err.error?.message || err.error?.title || 'An error occurred while saving.';
    window.scrollTo({ top: 0, behavior: 'smooth' });
    console.error(err);
  }
}