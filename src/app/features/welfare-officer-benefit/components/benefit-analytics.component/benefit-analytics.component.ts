import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BenefitService } from '../../services/benefit.service';
import { AnalyticsDashboardViewModel } from '../../models/benefit.model';
import { BenefitNavbarComponent } from '../benefit-navbar.component/benefit-navbar.component';
@Component({
  selector: 'app-benefit-analytics',
  standalone: true,
  imports: [CommonModule, BenefitNavbarComponent],
  templateUrl: './benefit-analytics.component.html',
  styleUrls: ['./benefit-analytics.component.css']
})
export class BenefitAnalyticsComponent implements OnInit {
  private benefitService = inject(BenefitService);

  dashboardData: AnalyticsDashboardViewModel | null = null;
  isLoading = true;
  hasError = false;

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.isLoading = true;
    this.hasError = false;

    this.benefitService.getAnalyticsDashboard().subscribe({
      next: (data) => {
        this.dashboardData = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error fetching dashboard data:', err);
        this.hasError = true;
        this.isLoading = false;
      }
    });
  }

  // Helper for Status Badges
  getStatusClass(status: string): string {
    const s = status ? status.toLowerCase() : '';
    if (s === 'completed' || s === 'fully disbursed') return 'status-completed';
    if (s === 'failed') return 'status-failed';
    return 'status-pending';
  }

  // Helper for Activity Icons
  getIconBackground(status: string): string {
    const s = status ? status.toLowerCase() : '';
    if (s === 'completed' || s === 'fully disbursed') return 'bg-success';
    if (s === 'failed') return 'status-failed bg-danger';
    return 'bg-warning text-dark';
  }

  // Helper for Benefit Type Colors
  getTypeColor(type: string): string {
    const t = type ? type.toLowerCase() : '';
    switch (t) {
      case 'cash': return '#3b82f6';
      case 'food': return '#f59e0b';
      case 'medical': return '#ef4444';
      case 'education': return '#8b5cf6';
      case 'housing': return '#10b981';
      default: return '#64748b';
    }
  }

  // Helpers for Monthly Trends Bar Chart calculation
  getMaxTrendValue(): number {
    if (!this.dashboardData?.monthlyTrends || this.dashboardData.monthlyTrends.length === 0) return 1;
    const maxAllocated = Math.max(...this.dashboardData.monthlyTrends.map(t => t.allocatedAmount));
    const maxDisbursed = Math.max(...this.dashboardData.monthlyTrends.map(t => t.disbursedAmount));
    const max = Math.max(maxAllocated, maxDisbursed);
    return max === 0 ? 1 : max;
  }

  getBarHeight(amount: number): number {
    const max = this.getMaxTrendValue();
    return Math.round((amount / max) * 100);
  }
}