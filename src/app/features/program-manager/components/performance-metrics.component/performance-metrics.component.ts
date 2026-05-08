import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ProgramManagerService } from '../../services/program-manager.service';
import { ProgramPerformance } from '../../models/program.model';
import { ProgramManagerNavbarComponent } from '../program-manager-navbar.component/program-manager-navbar.component';

@Component({
  selector: 'app-performance-metrics',
  standalone: true,
  imports: [CommonModule, RouterModule, ProgramManagerNavbarComponent],
  templateUrl: './performance-metrics.component.html'
})
export class PerformanceMetricsComponent implements OnInit {
  private programService = inject(ProgramManagerService);

  performances: ProgramPerformance[] = [];
  isLoading = true;

  // Summary Totals
  totalPrograms = 0;
  activePrograms = 0;
  totalApplications = 0;
  totalDisbursed = 0;

  ngOnInit(): void {
    this.programService.getPerformanceMetrics().subscribe({
      next: (data) => {
        this.performances = data;

        this.totalPrograms = data.length;
        this.activePrograms = data.filter(p => p.status === 'Active').length;
        this.totalApplications = data.reduce((sum, item) => sum + item.totalApplications, 0);
        this.totalDisbursed = data.reduce((sum, item) => sum + item.benefitsDisbursed, 0);

        this.isLoading = false;
      },
      error: (err) => {
        console.error(err);
        this.isLoading = false;
      }
    });
  }
}