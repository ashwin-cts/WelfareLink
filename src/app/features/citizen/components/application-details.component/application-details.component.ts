import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { CitizenService } from '../../services/citizen.service';
import { WelfareApplication, Benefit, Disbursement } from '../../../Gov-auditor/models/auditor.model';

@Component({
  selector: 'app-citizen-application-details',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './application-details.component.html',
  styleUrls: ['./application-details.component.css']
})
export class ApplicationDetailsComponent implements OnInit {
  applicationId!: number;
  application: WelfareApplication | null = null;
  isLoading = true;
  errorMessage = '';

  // Calculated Totals
  totalApprovedBenefit = 0;
  totalDisbursedAmount = 0;
  remainingPendingAmount = 0;
  
  // Flattened array for the Disbursement Table
  allDisbursements: Disbursement[] = [];

  constructor(
    private route: ActivatedRoute,
    private citizenService: CitizenService
  ) {}

  ngOnInit(): void {
    this.applicationId = Number(this.route.snapshot.paramMap.get('id'));
    if (this.applicationId) {
      this.loadApplicationData();
    }
  }

  loadApplicationData() {
    this.isLoading = true;
    this.citizenService.getApplicationDetails(this.applicationId).subscribe({
      next: (data) => {
        this.application = data;
        this.calculateFinancials();
        this.isLoading = false;
      },
      error: (err) => {
        this.errorMessage = 'Failed to load application details.';
        this.isLoading = false;
        console.error(err);
      }
    });
  }

  calculateFinancials() {
    if (!this.application || !this.application.benefits) return;

    // 1. Calculate Total Benefits Approved (e.g., 1k + 1k = 2k)
    this.totalApprovedBenefit = this.application.benefits.reduce((sum, benefit) => sum + benefit.amount, 0);

    // 2. Extract all disbursements from all benefits into one flat array
    this.allDisbursements = this.application.benefits.flatMap(benefit => benefit.disbursements || []);

    // 3. Calculate Total Disbursed (e.g., 500 + 500 + 250 + 250 = 1.5k)
    this.totalDisbursedAmount = this.allDisbursements.reduce((sum, disb) => sum + disb.amount, 0);

    // 4. Calculate Remaining Pending
    this.remainingPendingAmount = this.totalApprovedBenefit - this.totalDisbursedAmount;
  }

  // Helper for UI Progress Bar
  getDisbursementPercentage(): number {
    if (this.totalApprovedBenefit === 0) return 0;
    return (this.totalDisbursedAmount / this.totalApprovedBenefit) * 100;
  }
}