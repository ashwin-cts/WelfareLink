import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { ComplianceOfficerService } from '../../services/compliance-officer';
import { ApplicationDetail, ProgramResourcesDto } from '../../models/compliance-officer';

@Component({
  selector: 'app-compliance-application-details',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './compliance-application-details.component.html',
  styleUrls: ['./compliance-application-details.component.css']
})
export class ComplianceApplicationDetailsComponent implements OnInit {
  applicationId!: number;
  application?: ApplicationDetail;
  resources?: ProgramResourcesDto;
  isLoading = true;
  error = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private complianceService: ComplianceOfficerService
  ) {}

  ngOnInit() {
    this.applicationId = Number(this.route.snapshot.paramMap.get('id'));
    if (!this.applicationId || Number.isNaN(this.applicationId)) {
      this.error = 'Invalid application selected.';
      this.isLoading = false;
      return;
    }

    this.loadApplicationDetails();
  }

  loadApplicationDetails() {
    this.isLoading = true;
    this.error = '';

    this.complianceService.getApplicationDetails(this.applicationId).subscribe({
      next: (detail) => {
        this.application = detail;
        if (detail.ProgramID) {
          this.loadProgramResources(detail.ProgramID);
        } else {
          this.isLoading = false;
        }
      },
      error: (err) => {
        console.error(err);
        this.error = 'Unable to load application details. Please try again later.';
        this.isLoading = false;
      }
    });
  }

  loadProgramResources(programId: number) {
    this.complianceService.getProgramResources(programId).subscribe({
      next: (resources) => {
        this.resources = resources;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  backToDashboard() {
    this.router.navigate(['/compliance-dashboard']);
  }
}
