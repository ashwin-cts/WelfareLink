import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { DisbursementService } from '../../services/disbursement.service';
import { Disbursement, BenefitDetails } from '../../models/disbursement.model';
import { DisbursementNavbarComponent } from '../disbursement-navbar.component/disbursement-navbar.component';
@Component({
  selector: 'app-disbursement-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, DisbursementNavbarComponent],
  templateUrl: './disbursement-details.component.html',
  styleUrls: ['./disbursement-details.component.css']
})
export class DisbursementDetailComponent implements OnInit {
  private disbursementService = inject(DisbursementService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  disbursement: Disbursement | null = null;
  benefitSummary: BenefitDetails | null = null;
  siblingDisbursements: Disbursement[] = [];
  
  isLoading = true;
  errorMessage = '';

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const id = Number(params.get('id'));
      if (id) {
        this.loadDetails(id);
      }
    });
  }

  loadDetails(id: number): void {
    this.isLoading = true;
    this.disbursementService.getDisbursementById(id).subscribe({
      next: (data) => {
        this.disbursement = data;
        
        // Fetch extra context if this is tied to a benefit
        if (data.benefitID) {
          this.loadBenefitContext(data.benefitID);
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

  loadBenefitContext(benefitId: number): void {
    // 1. Load the overall budget/resource summary
    this.disbursementService.getBenefitDetails(benefitId).subscribe({
      next: (summary) => this.benefitSummary = summary
    });

    // 2. Load other disbursements linked to this exact same benefit
    this.disbursementService.getSiblingDisbursements(benefitId).subscribe({
      next: (siblings) => {
        // Filter out the current one so we only show the *other* ones
        this.siblingDisbursements = siblings.filter(s => s.disbursementID !== this.disbursement?.disbursementID);
        this.isLoading = false;
      },
      error: () => this.isLoading = false // Non-critical failure
    });
  }

  deleteDisbursement(): void {
    if (!this.disbursement) return;
    
    // Modernization: Replaces the entire Delete.cshtml page with a clean dialog
    const confirmed = window.confirm(`WARNING: Are you sure you want to permanently delete Disbursement #${this.disbursement.disbursementID}?\n\nThis action cannot be undone.`);
    
    if (confirmed) {
      this.disbursementService.deleteDisbursement(this.disbursement.disbursementID).subscribe({
        next: () => this.router.navigate(['/disbursements']),
        error: () => alert('Failed to delete the record. It may be locked or already deleted.')
      });
    }
  }

  getStatusClass(status: string | null): string {
    if (!status) return 'bg-secondary';
    const lower = status.toLowerCase();
    if (lower.includes('completed')) return 'bg-success';
    if (lower.includes('pending')) return 'bg-warning text-dark';
    if (lower.includes('failed')) return 'bg-danger';
    return 'bg-secondary';
  }
}