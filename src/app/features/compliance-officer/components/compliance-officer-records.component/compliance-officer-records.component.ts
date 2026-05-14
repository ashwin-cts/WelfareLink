import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ComplianceOfficerService } from '../../services/compliance-officer.service';
import { ComplianceRecord } from '../../models/compliance-officer.model';

@Component({
  selector: 'app-compliance-records',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="card shadow-sm">
      <div class="card-header bg-white"><h5 class="mb-0">Compliance Records</h5></div>
      <div class="card-body p-0">
        <table class="table table-hover align-middle mb-0">
          <thead class="bg-light">
            <tr><th>ID</th><th>Type</th><th>Violation</th><th>Description</th><th>Status</th><th>Action</th></tr>
          </thead>
          <tbody>
            <tr *ngFor="let record of records">
              <td>{{ record.recordID }}</td>
              <td>{{ record.entityType }} <small>(#{{ record.entityId }})</small></td>
              <td><span class="text-danger fw-bold">{{ record.violationType }}</span></td>
              <td>{{ record.description }}</td>
              <td>
                <span class="badge" [ngClass]="record.status === 'Open' ? 'bg-warning text-dark' : 'bg-success'">{{ record.status }}</span>
              </td>
              <td>
                <button *ngIf="record.status === 'Open'" class="btn btn-sm btn-success" (click)="resolve(record)">Clear Flag</button>
                <span *ngIf="record.status !== 'Open'" class="text-muted small">Resolved: {{ record.notes }}</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  `
})
export class ComplianceRecordsComponent implements OnInit {
  records: ComplianceRecord[] = [];
  constructor(private service: ComplianceOfficerService) {}

  ngOnInit() {
    this.service.getComplianceRecords().subscribe(res => this.records = res);
  }

  resolve(record: ComplianceRecord) {
    const notes = prompt('Enter notes for clearing this flag:');
    if (notes) {
      this.service.resolveComplianceIssue(record.recordID, notes).subscribe({
        next: () => { record.status = 'Resolved'; record.notes = notes; }
      });
    }
  }
}