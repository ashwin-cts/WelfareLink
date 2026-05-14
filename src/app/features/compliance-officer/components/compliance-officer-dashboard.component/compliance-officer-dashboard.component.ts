import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ComplianceOfficerService } from '../../services/compliance-officer.service';
import { DashboardApplication } from '../../models/compliance-officer.model';

@Component({
  selector: 'app-compliance-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="card shadow-sm border-0 mb-4">
      <div class="card-header bg-white py-3">
        <h5 class="mb-0 text-primary"><i class="bi bi-list-check me-2"></i>Applications for Compliance Review</h5>
      </div>
      <div class="card-body p-0">
        <div class="table-responsive">
          <table class="table table-hover align-middle mb-0">
            <thead class="bg-light">
              <tr>
                <th>App ID</th>
                <th>Citizen Details</th>
                <th>Program</th>
                <th>Max Benefit</th>
                <th>Total Allocated</th>
                <th>Total Disbursed</th>
                <th>Status</th>
                <th>Action</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngIf="applications.length === 0">
                <td colspan="8" class="text-center py-4 text-muted">Loading applications or no data available...</td>
              </tr>
              <tr *ngFor="let app of applications">
                <td><strong>APP-{{ app.applicationID }}</strong></td>
                <td>
                  {{ app.citizenName }}<br>
                  <small class="text-muted">ID: {{ app.citizenID }}</small>
                </td>
                <td>{{ app.programTitle }}</td>
                <td>{{ app.maxBenefit | currency:'INR' }}</td>
                <td class="text-success fw-bold">{{ app.totalBenefitAllocated | currency:'INR' }}</td>
                <td class="text-info fw-bold">{{ app.totalDisbursed | currency:'INR' }}</td>
                <td>
                  <span class="badge" [ngClass]="app.isFlagged ? 'bg-danger' : 'bg-success'">
                    {{ app.isFlagged ? 'Flagged' : 'Clear' }}
                  </span>
                </td>
                <td>
                  <button class="btn btn-sm btn-outline-primary" [routerLink]="['/compliance/application', app.applicationID]">
                    <i class="bi bi-eye"></i> View
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  `
})
export class ComplianceDashboardComponent implements OnInit {
  applications: DashboardApplication[] = [];

  constructor(private service: ComplianceOfficerService) {}

  ngOnInit() {
    this.service.getDashboardApplications().subscribe({
      next: (apps: DashboardApplication[]) => this.applications = apps,
      error: (err) => console.error("Error fetching dashboard apps:", err)
    });
  }
}