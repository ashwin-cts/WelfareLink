import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { BenefitService } from '../../services/benefit.service';
import { Benefit, ProgramResourceInfo, WelfareApplication } from '../../models/benefit.model';
import { BenefitNavbarComponent } from '../benefit-navbar.component/benefit-navbar.component';
import { Location } from '@angular/common';
@Component({
  selector: 'app-benefit-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, BenefitNavbarComponent],
  templateUrl: './benefit-form.component.html',
  styleUrls: ['./benefit-form.component.css']
})
export class BenefitFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private benefitService = inject(BenefitService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private location = inject(Location);
  benefitForm!: FormGroup;
  isEditMode: boolean = false;
  benefitId: number = 0;

  // STRICTLY TYPED: Replaced 'any' with your actual interfaces
  applicationsList: WelfareApplication[] = [];
  selectedAppDetails: WelfareApplication | undefined = undefined;
  resourceInfo: ProgramResourceInfo | null = null;

  isLoading: boolean = false;
  errorMessage: string = '';

  ngOnInit(): void {
    this.initForm();
    this.loadDropdownData();

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode = true;
      this.benefitId = Number(id);
      this.loadBenefitForEdit();
    }

    // Listen for dropdown changes
    this.benefitForm.get('applicationID')?.valueChanges.subscribe((appId: string | number) => {
      if (appId) {
        this.showAppDetails(Number(appId));
      } else {
        this.selectedAppDetails = undefined;
        this.resourceInfo = null;
      }
    });
  }

  private initForm(): void {
    this.benefitForm = this.fb.group({
      applicationID: ['', Validators.required],
      type: ['', Validators.required],
      amount: ['', [Validators.required, Validators.min(1)]],
      date: ['', Validators.required],
      status: ['Allocated', Validators.required]
    });
  }
  goBack(): void {
    this.location.back();
  }
  private loadDropdownData(): void {
    // We expect the API to return an array of WelfareApplication objects
    this.benefitService.getDropdownData().subscribe({
      next: (data: { applications: WelfareApplication[] } | WelfareApplication[]) => {
        // Handle both possible JSON structures from your backend
        this.applicationsList = Array.isArray(data) ? data : data.applications;
        console.log("🛑 RAW DATA FROM BACKEND: ", data);
      },
      error: (err: HttpErrorResponse) => this.handleBackendError(err)
    });
  }

  private loadBenefitForEdit(): void {
    this.isLoading = true;
    this.benefitService.getBenefitById(this.benefitId).subscribe({
      next: (benefit: Benefit) => {
        const formattedDate = new Date(benefit.date).toISOString().split('T')[0];

        this.benefitForm.patchValue({
          applicationID: benefit.applicationID,
          type: benefit.type,
          amount: benefit.amount,
          date: formattedDate,
          status: benefit.status
        });
        this.isLoading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.handleBackendError(err);
      }
    });
  }

  private showAppDetails(appId: number): void {
    // Ensure strict number matching so the box actually triggers
    this.selectedAppDetails = this.applicationsList.find(a => a.applicationID === appId);

    if (this.selectedAppDetails && this.selectedAppDetails.programID) {
      this.benefitService.getProgramResourceInfo(this.selectedAppDetails.programID)
        .subscribe({
          next: (info: ProgramResourceInfo) => this.resourceInfo = info,
          error: (err: HttpErrorResponse) => console.error("Failed to load resource info", err)
        });
    } else {
      this.resourceInfo = null;
    }
  }

  onSubmit(): void {
    if (this.benefitForm.invalid) {
      this.benefitForm.markAllAsTouched();
      return;
    }

    this.errorMessage = '';
    this.isLoading = true;

    const formValues = this.benefitForm.value;
    const payload: Benefit = {
      benefitID: this.isEditMode ? this.benefitId : 0,
      applicationID: Number(formValues.applicationID),
      type: formValues.type,
      amount: Number(formValues.amount),
      date: formValues.date,
      status: formValues.status
    };

    if (this.isEditMode) {
      this.benefitService.updateBenefit(this.benefitId, payload).subscribe({
        next: () => this.router.navigate(['/welfare-officer/benefit-list']),
        error: (err: HttpErrorResponse) => this.handleBackendError(err)
      });
    } else {
      const currentOfficerId = 1; // Or get from auth service
      this.benefitService.createBenefit(payload, currentOfficerId).subscribe({
        next: () => this.router.navigate(['/welfare-officer/benefit-list']),
        error: (err: HttpErrorResponse) => this.handleBackendError(err)
      });
    }
  }

  // Strictly Typed Error Handler
  private handleBackendError(err: HttpErrorResponse): void {
    this.isLoading = false;

    if (err.error && err.error.detail) {
      this.errorMessage = err.error.detail;
    } else if (err.error && err.error.errors) {
      const firstErrorKey = Object.keys(err.error.errors)[0];
      this.errorMessage = err.error.errors[firstErrorKey][0];
    } else if (err.error && typeof err.error === 'string') {
      this.errorMessage = err.error;
    } else if (err.error && err.error.message) {
      this.errorMessage = err.error.message;
    } else if (err.status === 400) {
      this.errorMessage = 'Bad Request: Please check your form data and try again.';
    } else {
      this.errorMessage = 'A server error occurred while trying to save. Please try again later.';
    }
  }
}