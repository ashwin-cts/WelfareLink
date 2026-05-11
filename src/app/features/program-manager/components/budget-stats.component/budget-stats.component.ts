import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ProgramManagerService } from '../../services/program-manager.service';
import { BudgetMonitoring } from '../../models/program.model';
import { ProgramManagerNavbarComponent } from '../program-manager-navbar.component/program-manager-navbar.component';

@Component({
  selector: 'app-budget-stats',
  standalone: true,
  imports: [CommonModule, RouterModule, ProgramManagerNavbarComponent],
  templateUrl: './budget-stats.component.html'
})
export class BudgetStatsComponent implements OnInit {
  private programService = inject(ProgramManagerService);

  budgets: BudgetMonitoring[] = [];
  isLoading = true;

  // Summary Totals
  totalBudget = 0;
  totalAllocated = 0;
  totalRemaining = 0;
  criticalCount = 0;

  ngOnInit(): void {
    this.programService.getBudgetMonitoring().subscribe({
      // 1. Change 'data' type to 'any' so we can unpack the C# ViewModel
      next: (data: any) => {

        // 2. The table data is inside the 'programBudgets' array property
        // We use || to handle both camelCase and PascalCase from the C# serializer
        this.budgets = data.programBudgets || data.ProgramBudgets || [];

        // 3. Instead of calculating with .reduce(), just grab the exact numbers 
        // the C# backend already calculated for the top cards!
        this.totalBudget = data.totalBudgetAllPrograms || data.TotalBudgetAllPrograms || 0;
        this.totalAllocated = data.totalAllocated || data.TotalAllocated || 0;
        this.totalRemaining = data.totalRemaining || data.TotalRemaining || 0;
        this.criticalCount = data.criticalProgramsCount || data.CriticalProgramsCount || 0;

        this.isLoading = false;
      },
      error: (err) => {
        console.error("Failed to load budget data:", err);
        this.isLoading = false;
      }
    });
  }
}
