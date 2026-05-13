import { Component, OnInit, inject } from '@angular/core';
import { CommonModule, Location } from '@angular/common'; // <-- Imported Location
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { DisbursementService } from '../../services/disbursement.service';
import { BenefitDetails, Disbursement } from '../../models/disbursement.model';
import { DisbursementNavbarComponent } from '../disbursement-navbar.component/disbursement-navbar.component';

@Component({
  selector: 'app-disbursement-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, DisbursementNavbarComponent],
  templateUrl: './disbursement-form.component.html'
})
export class DisbursementFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private disbursementService = inject(DisbursementService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private location = inject(Location); // <-- Injecting Location for the dynamic back button

  disbursementForm!: FormGroup;
  isEditMode = false;
  currentDisbursementId: number | null = null;

  // Data for the UI
  benefitDetails: any | null = null;
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
      next: (details: any) => {
        // Safe mapping in case the API wraps this one too!
        this.benefitDetails = details.benefitDetails || details;

        if (this.benefitDetails && this.benefitDetails.citizenId) {
          this.disbursementForm.patchValue({ citizenID: this.benefitDetails.citizenId });
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
      next: (response: any) => {
        // THE FIX: Unpack the wrapper object so the form actually gets the data!
        const data = response.disbursement || response;

        const formattedDate = data.date ? data.date.split('T')[0] : '';

        this.disbursementForm.patchValue({
          benefitID: data.benefitID,
          amount: data.amount,
          citizenID: data.citizenID,
          officerID: data.officerID,
          date: formattedDate,
          status: data.status
        });

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

    if (this.benefitDetails?.isResourceExhausted) {
      this.errorMessage = 'Cannot disburse. Programme resources are fully exhausted.';
      window.scrollTo({ top: 0, behavior: 'smooth' });
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';

    const formData = this.disbursementForm.getRawValue();

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
        next: () => this.router.navigate(['/welfare-officer/disbursement-list']), // FIXED ROUTE
        error: (err) => this.handleError(err)
      });
    } else {
      this.disbursementService.createDisbursement(payload).subscribe({
        next: () => this.router.navigate(['/welfare-officer/disbursement-list']), // FIXED ROUTE
        error: (err) => this.handleError(err)
      });
    }
  }

  // Uses the browser history to go exactly to the page they just came from!
  goBack(): void {
    this.location.back();
  }

  handleError(err: any): void {
    this.isSaving = false;
    this.errorMessage = err.error?.message || err.error?.title || 'An error occurred while saving.';
    window.scrollTo({ top: 0, behavior: 'smooth' });
    console.error(err);
  }
}