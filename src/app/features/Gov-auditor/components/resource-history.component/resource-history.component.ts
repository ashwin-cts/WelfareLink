import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ResourceStatementItem } from '../services/auditor.service';

@Component({
  selector: 'app-resource-history',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './resource-history.component.html'
})
export class ResourceHistoryComponent {
  @Input() items: ResourceStatementItem[] = [];
  @Output() print = new EventEmitter<void>();
  @Output() export = new EventEmitter<void>();

  getTotalAllocatedResources(): number {
    return this.items.reduce((sum, item) => sum + item.allocatedResource, 0);
  }
}