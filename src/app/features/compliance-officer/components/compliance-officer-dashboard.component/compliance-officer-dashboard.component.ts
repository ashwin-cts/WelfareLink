import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ComplianceOfficerService } from '../../services/compliance-officer.service';
import { DashboardApplication } from '../../models/compliance-officer.model';

@Component({
  selector: 'app-compliance-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './compliance-officer-dashboard.component.html',
  styleUrls: ["./compliance-officer-dashboard.component.css"]
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