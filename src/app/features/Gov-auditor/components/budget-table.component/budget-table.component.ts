import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BudgetMonitoringItem } from '../services/auditor.service';

@Component({
  selector: 'app-budget-table',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './budget-table.component.html'
})
export class BudgetTableComponent {
  @Input() items: BudgetMonitoringItem[] = [];

  getTotalBudget(): number {
    return this.items.reduce((sum, item) => sum + item.programBudget, 0);
  }

  getTotalDisbursed(): number {
    return this.items.reduce((sum, item) => sum + item.totalDisbursed, 0);
  }
}