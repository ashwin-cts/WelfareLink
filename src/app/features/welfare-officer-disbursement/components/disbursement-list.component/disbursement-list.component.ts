import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

// RxJS Imports for API chaining
import { forkJoin, of } from 'rxjs';
import { catchError, map, switchMap } from 'rxjs/operators';

import { DisbursementService } from '../../services/disbursement.service';
import { BenefitService } from '../../../welfare-officer-benefit/services/benefit.service'; // Make sure path is correct
import { WelfareOfficerService } from '../../../welfare-officer/services/welfare-officer.services'; // Make sure path is correct

import { Disbursement } from '../../models/disbursement.model';
import { DisbursementNavbarComponent } from '../disbursement-navbar.component/disbursement-navbar.component';

// Extend the Disbursement model to include the mapped strings
export interface EnrichedDisbursement extends Disbursement {
  citizenName?: string;
  programName?: string;
}

@Component({
  selector: 'app-disbursement-list',
  standalone: true,
  imports: [CommonModule, RouterModule, DisbursementNavbarComponent],
  templateUrl: './disbursement-list.component.html',
  styleUrls: ['./disbursement-list.component.css'] // Use .css if you prefer
})
export class DisbursementListComponent implements OnInit {
  private disbursementService = inject(DisbursementService);
  private benefitService = inject(BenefitService);
  private applicationService = inject(WelfareOfficerService);

  // Use the Enriched model here
  disbursements: EnrichedDisbursement[] = [];
  pendingCount = 0;
  isLoading = true;
  errorMessage = '';
  isSortDescending: boolean = true;

  ngOnInit(): void {
    this.loadDisbursements();
  }
  toggleDateSort() {
    this.isSortDescending = !this.isSortDescending;
    this.applySorting();
  }

  applySorting() {
    this.disbursements.sort((a, b) => {
      // Ensure 'a.date' matches the exact property name on your disbursement model
      const dateA = new Date(a.date).getTime();
      const dateB = new Date(b.date).getTime();

      // If descending, B - A (Newest first). If ascending, A - B (Oldest first)
      return this.isSortDescending ? (dateB - dateA) : (dateA - dateB);
    });
  }
  loadDisbursements(): void {
    this.isLoading = true;

    this.disbursementService.getDisbursements().pipe(
      switchMap(disbursements => {
        // If no disbursements returned, return empty array
        if (!disbursements || disbursements.length === 0) {
          return of([]);
        }

        // Sort by date descending (newest first)
        const sorted = disbursements.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime());

        // Chain the APIs: For every disbursement, fetch Benefit -> Application
        const enrichmentRequests = sorted.map(disb =>
          this.benefitService.getBenefitById(disb.benefitID).pipe(
            switchMap(benefit => this.applicationService.getApplicationById(benefit.applicationID)),
            map(application => ({
              ...disb,
              citizenName: application?.citizen?.name || 'Unknown Citizen',
              programName: application?.program?.title || 'Unknown Program'
            }) as EnrichedDisbursement),
            catchError(() => of({
              ...disb,
              citizenName: 'Data Unavailable',
              programName: 'Data Unavailable'
            } as EnrichedDisbursement)) // Fallback if API fails
          )
        );

        // Wait for all the individual requests to finish
        return forkJoin(enrichmentRequests);
      })
    ).subscribe({
      next: (enrichedData) => {
        this.disbursements = enrichedData;

        // Calculate pending items for the alert banner
        this.pendingCount = this.disbursements.filter(d =>
          d.status === 'Disbursement Pending' || d.status === 'Pending'
        ).length;
        this.applySorting();
        this.isLoading = false;
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