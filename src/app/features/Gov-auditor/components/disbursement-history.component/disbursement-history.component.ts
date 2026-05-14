import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DisbursementStatementItem } from '../../models/auditor.model';
import { AuditorService } from '../services/auditor.service';

@Component({
  selector: 'app-disbursement-history',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './disbursement-history.component.html'
})
export class DisbursementHistoryComponent {
  @Input() items: DisbursementStatementItem[] = [];
  @Output() print = new EventEmitter<void>();
  @Output() export = new EventEmitter<void>();
}