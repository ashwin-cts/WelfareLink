import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ProgramManagerService } from '../../services/program-manager.service';
import { WelfareProgram, Resource } from '../../models/program.model';
import { ProgramManagerNavbarComponent } from '../program-manager-navbar.component/program-manager-navbar.component';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-pm-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, ProgramManagerNavbarComponent],
  templateUrl: './pm-dashboard.component.html',
  styleUrls: ['./pm-dashboard.component.css']
})
export class PmDashboardComponent implements OnInit {
  private programService = inject(ProgramManagerService);

  programs: WelfareProgram[] = [];
  totalBudget: number = 0;
  allocatedBudget: number = 0;
  remainingBudget: number = 0;
  isLoading: boolean = true;

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.isLoading = true;

    // Using forkJoin to wait for both API calls
    forkJoin({
      programs: this.programService.getPrograms(),
      resources: this.programService.getResources()
    }).subscribe({
      next: (result) => {
        this.programs = result.programs;
        this.totalBudget = this.programs.reduce((sum, p) => sum + p.budget, 0);
        this.allocatedBudget = result.resources
          .filter(r => r.type.toLowerCase() === 'funds')
          .reduce((sum, r) => sum + r.quantity, 0);

        this.remainingBudget = this.totalBudget - this.allocatedBudget;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Dashboard load failed', err);
        this.isLoading = false;
      }
    });
  }
}