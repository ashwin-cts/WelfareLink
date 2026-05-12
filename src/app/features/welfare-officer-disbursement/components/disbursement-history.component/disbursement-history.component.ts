import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { DisbursementService } from '../../services/disbursement.service';
import { Disbursement } from '../../models/disbursement.model';

@Component({
  selector: 'app-disbursement-history',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './disbursement-history.component.html',
  styleUrls: ['./disbursement-history.component.css']
})
export class DisbursementHistoryComponent implements OnInit {
  private fb = inject(FormBuilder);
  private disbursementService = inject(DisbursementService);

  filterForm!: FormGroup;
  historyData: Disbursement[] = [];
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
      officerId: [''],
      status: ['']
    });

    // Load initial unfiltered history
    this.loadData();
  }

  loadData(): void {
    this.isLoading = true;
    const filters = this.filterForm.value;

    this.disbursementService.filterDisbursements(filters).subscribe({
      next: (data) => {
        this.historyData = data.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime());
        this.calculateStats();
        this.isLoading = false;
      },
      error: () => this.isLoading = false
    });
  }

  calculateStats(): void {
    this.totalCount = this.historyData.length;
    this.completedCount = this.historyData.filter(d => d.status?.toLowerCase().includes('completed')).length;
    this.pendingCount = this.historyData.filter(d => d.status?.toLowerCase().includes('pending')).length;
    this.failedCount = this.historyData.filter(d => d.status?.toLowerCase().includes('failed')).length;
  }

  resetFilters(): void {
    this.filterForm.reset();
    this.loadData();
  }
}