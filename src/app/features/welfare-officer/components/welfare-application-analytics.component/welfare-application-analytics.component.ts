import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { forkJoin } from 'rxjs';

import {
  AppAnalyticsDashboard,
  AppStatusBreakdown,
  AppMonthlyTrendData,
  AppEligibilityReport,
  AppEligibilityCheckMonth,
  AppApprovalRate
} from '../../models/welfare-officer.models';
import { WelfareOfficerService } from '../../services/welfare-officer.services';
import { WelfareApplicationNavbarComponent } from '../welfare-application-navbar.component/welfare-application-navbar.component';
@Component({
  selector: 'app-welfare-application-analytics',
  standalone: true,
  imports: [CommonModule, WelfareApplicationNavbarComponent],
  templateUrl: './welfare-application-analytics.component.html',
  styleUrls: ['./welfare-application-analytics.component.css']
})
export class WelfareApplicationAnalyticsComponent implements OnInit {
  private analyticsService = inject(WelfareOfficerService);

  // Strictly typed data variables based on real API
  dashboardData: AppAnalyticsDashboard | null = null;
  approvalRate: AppApprovalRate | null = null;
  statusBreakdown: AppStatusBreakdown[] = [];

  // Notice how we extract the arrays from the wrapper objects
  monthlyTrends: AppMonthlyTrendData[] = [];
  trendYear: number = new Date().getFullYear();

  eligibilityReport: AppEligibilityReport | null = null;
  eligibilityMonthly: AppEligibilityCheckMonth[] = [];

  isLoading = true;
  hasError = false;
  activeTab: 'dashboard' | 'status' | 'trends' | 'eligibility' = 'dashboard';

  ngOnInit(): void {
    this.loadAllData();
  }

  loadAllData(): void {
    this.isLoading = true;
    this.hasError = false;

    forkJoin({
      dashboard: this.analyticsService.getApplicationDashboard(),
      approval: this.analyticsService.getApprovalRate(),
      status: this.analyticsService.getStatusBreakdown(),
      trends: this.analyticsService.getMonthlyTrends(),
      eligibility: this.analyticsService.getEligibilityReport()
    }).subscribe({
      next: (results) => {
        this.dashboardData = results.dashboard;
        this.approvalRate = results.approval;
        this.statusBreakdown = results.status;

        // Extracting data from the wrapper objects sent by the API!
        this.monthlyTrends = results.trends.MonthlyData;
        this.trendYear = results.trends.Year;

        this.eligibilityReport = results.eligibility;
        this.eligibilityMonthly = results.eligibility.ChecksByMonth;

        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading application analytics:', err);
        this.hasError = true;
        this.isLoading = false;
      }
    });
  }

  getStatusColorClass(status: string): string {
    const s = status ? status.toLowerCase() : '';
    if (s.includes('approved') || s.includes('eligible')) return 'bg-success';
    if (s.includes('rejected') || s.includes('ineligible')) return 'bg-danger';
    if (s.includes('review') || s.includes('progress')) return 'bg-info';
    return 'bg-warning text-dark';
  }
  getMaxTrendValue(): number {
    if (!this.monthlyTrends || this.monthlyTrends.length === 0) return 1;
    // Find the highest total applications in any month to scale the chart
    const max = Math.max(...this.monthlyTrends.map(t => t.total));
    return max === 0 ? 1 : max;
  }

  getBarHeight(amount: number): number {
    const max = this.getMaxTrendValue();
    return Math.round((amount / max) * 100);
  }
}