import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuditorDashboardStats } from '../../models/auditor.model';

@Component({
  selector: 'app-summary-cards',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './summary-cards.component.html'
})
export class SummaryCardsComponent {
  // Receives data from the parent
  @Input() isLoading: boolean = true;
  @Input() stats: AuditorDashboardStats | null = null;

  // Emits an event to the parent when a quick action button is clicked
  @Output() changeTab = new EventEmitter<'budget' | 'resource' | 'disbursement'>();

  onTabClick(tab: 'budget' | 'resource' | 'disbursement') {
    this.changeTab.emit(tab);
  }
}