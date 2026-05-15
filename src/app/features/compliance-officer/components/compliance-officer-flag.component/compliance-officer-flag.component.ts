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
  templateUrl:'./compliance-officer-flag.component.html',
  styleUrls: ['./compliance-officer-flag.component.css']

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