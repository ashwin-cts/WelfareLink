import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { BenefitService } from '../../services/benefit.service';
import { Benefit, Disbursement } from '../../models/benefit.model';
import { BenefitNavbarComponent } from '../benefit-navbar.component/benefit-navbar.component';
@Component({
  selector: 'app-benefit-list',
  standalone: true,
  imports: [CommonModule, RouterModule, BenefitNavbarComponent],
  templateUrl: './benefit-list.component.html',
  styleUrls: ['./benefit-list.component.css']
})
export class BenefitListComponent implements OnInit {
  private benefitService = inject(BenefitService);

  benefits: Benefit[] = [];
  isLoading = true;

  ngOnInit(): void {
    this.loadBenefits();
  }

  loadBenefits(): void {
    this.isLoading = true;
    this.benefitService.getAllBenefits().subscribe({
      next: (data) => {
        // --- ADD THESE TWO LINES ---
        console.log('FULL API RESPONSE:', data);
        if (data.length > 0) console.log('FIRST ITEM DETAILS:', data[0]);
        // ---------------------------

        this.benefits = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error fetching benefits', err);
        this.isLoading = false;
      }
    });
  }

  // Translating your C# Razor status logic into TypeScript
  getDisplayStatus(benefit: Benefit): string {
    const hasFailed = this.checkFailedDisbursements(benefit.disbursements);
    return hasFailed ? 'Failed' : (benefit.status || 'Pending');
  }

  getStatusClass(benefit: Benefit): string {
    if (this.checkFailedDisbursements(benefit.disbursements)) {
      return 'status-failed';
    }

    const statusLower = (benefit.status || '').toLowerCase().trim();
    if (statusLower.includes('fully') && statusLower.includes('disbursed')) return 'status-fully-disbursed';
    if (statusLower.includes('partially') && statusLower.includes('disbursed')) return 'status-partial';
    if (statusLower.includes('allocated')) return 'status-allocated';
    if (statusLower.includes('failed') || statusLower.includes('rejected')) return 'status-failed';

    return 'status-pending';
  }

  // Add this method to your component class
  deleteBenefit(id: number): void {
    const confirmDelete = window.confirm(`Are you sure you want to permanently delete Benefit #${id}? This action cannot be undone.`);

    if (confirmDelete) {
      this.benefitService.deleteBenefit(id).subscribe({
        next: () => {
          // Instantly refresh the table to show the item is gone!
          this.loadBenefits();
        },
        error: (err) => {
          console.error('Error deleting benefit', err);
          alert('Failed to delete the benefit. Please try again.');
        }
      });
    }
  }
  private checkFailedDisbursements(disbursements?: Disbursement[]): boolean {
    if (!disbursements || disbursements.length === 0) return false;
    return disbursements.some(d => d.status && d.status.toLowerCase().includes('failed'));
  }
}