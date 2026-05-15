import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { RouterModule } from '@angular/router';

// RxJS Imports for chaining API calls
import { forkJoin, of } from 'rxjs';
import { catchError, map, switchMap } from 'rxjs/operators';

import { DisbursementService } from '../../services/disbursement.service';
import { BenefitService } from '../../../welfare-officer-benefit/services/benefit.service'; // Make sure this path is correct
import { WelfareOfficerService } from '../../../welfare-officer/services/welfare-officer.services'; // Make sure this path is correct

import { Disbursement } from '../../models/disbursement.model';
import { DisbursementNavbarComponent } from '../disbursement-navbar.component/disbursement-navbar.component';

// 1. Extend your Disbursement model to include the newly mapped strings
export interface EnrichedDisbursement extends Disbursement {
  citizenName?: string;
  programName?: string;
}

@Component({
  selector: 'app-disbursement-history',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, DisbursementNavbarComponent],
  templateUrl: './disbursement-history.component.html',
  styleUrls: ['./disbursement-history.component.css']
})
export class DisbursementHistoryComponent implements OnInit {
  private fb = inject(FormBuilder);
  private disbursementService = inject(DisbursementService);

  // 2. Inject the necessary services for chaining
  private benefitService = inject(BenefitService);
  private applicationService = inject(WelfareOfficerService);

  filterForm!: FormGroup;
  // Use the Enriched model here
  historyData: EnrichedDisbursement[] = [];
  isLoading = true;

  // Stats
  totalCount = 0;
  completedCount = 0;
  pendingCount = 0;
  failedCount = 0;

  ngOnInit(): void {
    this.filterForm = this.fb.group({
      startDate: [''],
      endDate: [''],
      benefitType: [''],
      status: ['']
    });

    this.loadData();
  }

  loadData(): void {
    this.isLoading = true;
    const filters = this.filterForm.value;

    this.disbursementService.filterDisbursements(filters).pipe(
      switchMap(disbursements => {
        // If no disbursements returned, immediately return an empty array
        if (!disbursements || disbursements.length === 0) {
          return of([]);
        }

        // Sort them by date first
        const sorted = disbursements.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime());

        // 3. Chain the APIs: For every disbursement, fetch Benefit -> Application
        const enrichmentRequests = sorted.map(disb =>
          this.benefitService.getBenefitById(disb.benefitID).pipe(
            switchMap(benefit =>
              // Once we have the benefit, fetch the Application to get Citizen and Program
              this.applicationService.getApplicationById(benefit.applicationID)
            ),
            map(application => {
              // Stitch the string names onto the disbursement object
              return {
                ...disb,
                citizenName: application?.citizen?.name || 'Unknown Citizen',
                programName: application?.program?.title || 'Unknown Program'
              } as EnrichedDisbursement;
            }),
            catchError(() => {
              // Fallback in case an API call fails so the whole table doesn't crash
              return of({
                ...disb,
                citizenName: 'Data Unavailable',
                programName: 'Data Unavailable'
              } as EnrichedDisbursement);
            })
          )
        );

        // Run all these requests in parallel and wait for them to finish
        return forkJoin(enrichmentRequests);
      })
    ).subscribe({
      next: (enrichedData) => {
        this.historyData = enrichedData;
        this.calculateStats();
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error fetching data:', err);
        this.isLoading = false;
      }
    });
  }

  calculateStats(): void {
    this.totalCount = this.historyData.length;
    this.completedCount = this.historyData.filter(d => d.status?.toLowerCase().includes('completed')).length;
    this.pendingCount = this.historyData.filter(d => d.status?.toLowerCase().includes('pending')).length;
    this.failedCount = this.historyData.filter(d => d.status?.toLowerCase().includes('failed')).length;
  }

  resetFilters(): void {
    this.filterForm.reset({
      startDate: '',
      endDate: '',
      benefitType: '',
      status: ''
    });
    this.loadData();
  }
}