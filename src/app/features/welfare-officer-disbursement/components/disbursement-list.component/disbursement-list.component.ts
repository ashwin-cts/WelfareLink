import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { DisbursementService } from '../../services/disbursement.service';
import { Disbursement } from '../../models/disbursement.model';
import { DisbursementNavbarComponent } from '../disbursement-navbar.component/disbursement-navbar.component';
@Component({
  selector: 'app-disbursement-list',
  standalone: true,
  imports: [CommonModule, RouterModule, DisbursementNavbarComponent],
  templateUrl: './disbursement-list.component.html',
  styleUrls: ['./disbursement-list.component.css'] // Use .css if you prefer
})
export class DisbursementListComponent implements OnInit {
  private disbursementService = inject(DisbursementService);

  disbursements: Disbursement[] = [];
  pendingCount = 0;
  isLoading = true;
  errorMessage = '';

  ngOnInit(): void {
    this.loadDisbursements();
  }

  loadDisbursements(): void {
    this.isLoading = true;
    this.disbursementService.getDisbursements().subscribe({
      next: (data) => {
        // Sort by date descending (newest first)
        this.disbursements = data.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime());

        // Calculate pending items for the alert banner
        this.pendingCount = this.disbursements.filter(d =>
          d.status === 'Disbursement Pending' || d.status === 'Pending'
        ).length;

        this.isLoading = false;
        console.log(data);
      },
      error: (err) => {
        this.errorMessage = 'Failed to load disbursements. Please try again later.';
        this.isLoading = false;
        console.error(err);
      }
    });
  }

  // A helper method to determine CSS classes for the status badges
  getStatusClass(status: string | null): string {
    if (!status) return 'bg-secondary';

    const lowerStatus = status.toLowerCase();
    if (lowerStatus.includes('completed') || lowerStatus.includes('fully disbursed')) return 'bg-success';
    if (lowerStatus.includes('pending')) return 'bg-warning text-dark';
    if (lowerStatus.includes('failed')) return 'bg-danger';
    if (lowerStatus.includes('partial')) return 'bg-info text-dark';

    return 'bg-secondary';
  }
}