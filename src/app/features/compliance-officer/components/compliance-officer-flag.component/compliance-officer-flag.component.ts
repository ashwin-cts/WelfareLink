import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ComplianceOfficerService } from '../../services/compliance-officer.service';
import { ApplicationDetail, BenefitDetail, DisbursementDetail } from '../../models/compliance-officer.model';

@Component({
  selector: 'app-flag-issue',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  template: `
    <div class="mx-auto" style="max-width: 700px;">
      
      <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h2 class="h4 mb-1"><i class="bi bi-exclamation-triangle text-danger me-2"></i>Raise Compliance Issue</h2>
          <p class="text-muted mb-0">Flag a violation for Application <!--#{{ applicationId }}--></p>
        </div>
        <div>
          <button class="btn btn-outline-secondary shadow-sm" [routerLink]="['/compliance/application', applicationId]">
            <i class="bi bi-arrow-left me-1"></i> Back to Application
          </button>
        </div>
      </div>

      <div class="card shadow-sm border-0">
        <div class="card-body p-4">
          <div *ngIf="successMsg" class="alert alert-success"><i class="bi bi-check-circle me-2"></i>{{ successMsg }}</div>
          
          <div class="alert alert-info mb-4">
            <i class="bi bi-info-circle me-2"></i> <strong>Context:</strong> You are raising a compliance violation for an entity linked to this application.
          </div>

          <div class="mb-3">
            <label class="form-label fw-bold text-secondary">Flagging Entity Level</label>
            <div>
              <div class="form-check form-check-inline">
                <input class="form-check-input" type="radio" name="entityType" id="typeBenefit" value="Benefit" [(ngModel)]="entityType">
                <label class="form-check-label" for="typeBenefit">Benefit Allocation</label>
              </div>
              <div class="form-check form-check-inline">
                <input class="form-check-input" type="radio" name="entityType" id="typeDisbursement" value="Disbursement" [(ngModel)]="entityType">
                <label class="form-check-label" for="typeDisbursement">Disbursement</label>
              </div>
            </div>
          </div>

          <div class="row mb-3">
            <div class="col-md-12" *ngIf="entityType === 'Benefit'">
              <label class="form-label fw-bold text-secondary">Select Benefit</label>
              <select class="form-select bg-light" [(ngModel)]="selectedEntityId">
                <option [ngValue]="null">-- Select Benefit --</option>
                <option *ngFor="let b of availableBenefits" [ngValue]="b.benefitID">
                  Benefit #<!--{{ b.benefitID }}-->{{ b.type }} - ({{ b.amount | currency:'INR' }})
                </option>
              </select>
            </div>

            <div class="col-md-12" *ngIf="entityType === 'Disbursement'">
              <label class="form-label fw-bold text-secondary">Select Disbursement</label>
              <select class="form-select bg-light" [(ngModel)]="selectedEntityId">
                <option [ngValue]="null">-- Select Disbursement --</option>
                <option *ngFor="let d of availableDisbursements" [ngValue]="d.disbursementID">
                  Disbursement #<!--{{ d.disbursementID }}-->{{ d.status }} - ({{ d.amount | currency:'INR' }})
                </option>
              </select>
            </div>
          </div>

          <div class="mb-3">
            <label class="form-label fw-bold text-secondary">Violation Type</label>
            <select class="form-select bg-light" [(ngModel)]="violationType">
              <option value="">-- Select --</option>
              <option>AmountExceedsBudget</option>
              <option>AmountMismatch</option>
              <option>DisbursementWrong</option>
              <option>BenefitAllocationWrong</option>
              <option>NoEligibilityCheck</option>
              <option>UnverifiedDocApproved</option>
              <option>DocumentMissing</option>
              <option>DuplicateApplication</option>
              <option>MoreThan2DaysSinceApproval</option>
              <option>Other</option>
            </select>
          </div>

          <div class="mb-4">
            <label class="form-label fw-bold text-secondary">Description</label>
            <textarea class="form-control bg-light" rows="4" [(ngModel)]="description" placeholder="Describe the violation in detail..."></textarea>
          </div>

          <div class="d-flex justify-content-end gap-2 mt-4 pt-3 border-top">
            <button class="btn btn-light shadow-sm border" [routerLink]="['/compliance/application', applicationId]">
              <i class="bi bi-x-circle me-1"></i> Cancel
            </button>
            <button class="btn btn-danger shadow-sm px-4" [disabled]="!selectedEntityId || !violationType || isSubmitting" (click)="submitFlag()">
              <span *ngIf="isSubmitting" class="spinner-border spinner-border-sm me-2"></span>
              <i class="bi bi-flag-fill me-1" *ngIf="!isSubmitting"></i> Raise Violation
            </button>
          </div>

        </div>
      </div>
    </div>
  `
})
export class FlagIssueComponent implements OnInit {
  applicationId!: number;
  entityType: 'Benefit' | 'Disbursement' = 'Benefit';
  selectedEntityId: number | null = null;
  violationType = ''; 
  description = '';
  
  availableBenefits: BenefitDetail[] = []; 
  availableDisbursements: DisbursementDetail[] = [];
  
  isSubmitting = false; 
  successMsg = '';

  constructor(
    private route: ActivatedRoute, 
    private service: ComplianceOfficerService, 
    private router: Router
  ) {}

  ngOnInit() {
    this.applicationId = Number(this.route.snapshot.paramMap.get('id'));
    
    this.service.getApplicationDetails(this.applicationId).subscribe((app: ApplicationDetail) => {
      this.availableBenefits = app.benefits || [];
      
      this.availableBenefits.forEach(b => {
        if (b.disbursements) {
          this.availableDisbursements.push(...b.disbursements);
        }
      });
    });
  }

  submitFlag() {
    if (!this.selectedEntityId) return;
    this.isSubmitting = true;
    
    const payload = { violationType: this.violationType, description: this.description };
    
    const req$ = this.entityType === 'Benefit' 
      ? this.service.raiseComplianceForAllocation(this.selectedEntityId, payload)
      : this.service.raiseComplianceForDisbursement(this.selectedEntityId, payload);

    req$.subscribe({
      next: () => {
        this.successMsg = 'Issue flagged successfully!';
        setTimeout(() => this.router.navigate(['/compliance/records']), 1500);
      },
      error: (err) => {
        console.error('Error submitting flag:', err);
        this.isSubmitting = false;
      }
    });
  }
}