import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { DisbursementService } from '../../services/disbursement.service';
import { DisbursementNavbarComponent } from '../disbursement-navbar.component/disbursement-navbar.component';
import { Location } from '@angular/common';
@Component({
  selector: 'app-disbursement-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, DisbursementNavbarComponent],
  templateUrl: './disbursement-details.component.html'
})
export class DisbursementDetailComponent implements OnInit {
  private disbursementService = inject(DisbursementService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private location = inject(Location);
  disbursement: any | null = null;
  benefitSummary: any | null = null;
  siblingDisbursements: any[] = [];

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
      next: (data: any) => {
        // 1. Extract the main disbursement
        this.disbursement = data.disbursement;

        // 2. Extract the siblings directly from the wrapper
        this.siblingDisbursements = data.siblingDisbursements || [];

        // 3. Extract the budget directly from the wrapper
        this.benefitSummary = {
          totalResource: data.benefitTotalAmount || 0,
          totalDisbursedForProgram: data.totalDisbursed || 0,
          availableResource: data.pendingBalance || 0,
          isResourceExhausted: (data.pendingBalance || 0) <= 0
        };

        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load disbursement details.';
        this.isLoading = false;
      }
    });
  }

  deleteDisbursement(): void {
    if (!this.disbursement) return;
    const confirmed = window.confirm(`WARNING: Are you sure you want to permanently delete Disbursement #${this.disbursement.disbursementID}?`);
    if (confirmed) {
      this.disbursementService.deleteDisbursement(this.disbursement.disbursementID).subscribe({
        next: () => this.router.navigate(['/welfare-officer/disbursement-list']),
        error: () => alert('Failed to delete the record.')
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
  goBack(): void {
    this.location.back();
  }
}