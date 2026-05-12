import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { BenefitService } from '../../services/benefit.service';
import { Benefit, ProgramResourceInfo } from '../../models/benefit.model';
import { BenefitNavbarComponent } from '../benefit-navbar.component/benefit-navbar.component';
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

  benefitForm!: FormGroup;
  isEditMode = false;
  benefitId = 0;

  // Data for dropdowns and display boxes
  applicationsList: any[] = []; // You'll populate this from your dropdown API
  selectedAppDetails: any = null;
  resourceInfo: ProgramResourceInfo | null = null;
  isLoading = false;

  ngOnInit(): void {
    // 1. Initialize the form structure and validators
    this.initForm();

    // 2. Load the dropdown data (Welfare Applications)
    this.loadDropdownData();

    // 3. Check if we are in Edit Mode
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode = true;
      this.benefitId = Number(id);
      this.loadBenefitForEdit();
    }

    // 4. Listen for changes to the Application dropdown (replaces your C# event listener)
    this.benefitForm.get('applicationID')?.valueChanges.subscribe(appId => {
      if (appId) {
        this.showAppDetails(Number(appId));
      } else {
        this.selectedAppDetails = null;
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
      status: ['Allocated', Validators.required] // Default to Allocated
    });
  }

  private loadDropdownData(): void {
    // Calling the dropdown endpoint you defined in the Swagger schema
    this.benefitService.getDropdownData().subscribe(data => {
      // Assuming the API returns a list of applications in 'data'
      this.applicationsList = data.applications || data;
    });
  }

  private loadBenefitForEdit(): void {
    this.isLoading = true;
    this.benefitService.getBenefitById(this.benefitId).subscribe(benefit => {
      // Patch the form with the existing data
      // Note: date formatting might be needed depending on how your HTML date input expects it (YYYY-MM-DD)
      const formattedDate = new Date(benefit.date).toISOString().split('T')[0];

      this.benefitForm.patchValue({
        applicationID: benefit.applicationID,
        type: benefit.type,
        amount: benefit.amount,
        date: formattedDate,
        status: benefit.status
      });
      this.isLoading = false;
    });
  }

  private showAppDetails(appId: number): void {
    // Find the application details from the dropdown list
    this.selectedAppDetails = this.applicationsList.find(a => a.applicationID === appId);

    // Fetch resource info if there's a linked program
    if (this.selectedAppDetails && this.selectedAppDetails.programID) {
      this.benefitService.getProgramResourceInfo(this.selectedAppDetails.programID)
        .subscribe(info => {
          this.resourceInfo = info;
        });
    }
  }

  onSubmit(): void {
    if (this.benefitForm.invalid) {
      this.benefitForm.markAllAsTouched();
      return;
    }

    const formValue: Benefit = { ...this.benefitForm.value };
    // In edit mode, we need to pass the ID back
    if (this.isEditMode) {
      formValue.benefitID = this.benefitId;
    }

    if (this.isEditMode) {
      this.benefitService.updateBenefit(this.benefitId, formValue).subscribe(() => {
        this.router.navigate(['../../'], { relativeTo: this.route });
      });
    } else {
      // Create mode - assuming an officerId is required, defaulting to 1 for example purposes
      const currentOfficerId = 1;
      this.benefitService.createBenefit(formValue, currentOfficerId).subscribe(() => {
        this.router.navigate(['../'], { relativeTo: this.route });
      });
    }
  }
}