import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ComplianceOfficerService } from '../../services/compliance-officer.service';
import { ComplianceRecord } from '../../models/compliance-officer.model';

@Component({
  selector: 'app-compliance-records',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './compliance-officer-records.component.html',
  styleUrls: ['./compliance-officer-records.component.css']
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