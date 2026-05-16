import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { ComplianceOfficerService } from '../../services/compliance-officer.service'; // Added EligibilityCheck import
import { ApplicationDetail, EligibilityCheck } from '../../models/compliance-officer.model';

@Component({
  selector: 'app-compliance-application-details',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './compliance-officer-application-list.component.html',
  styleUrls: ['./compliance-officer-application-list.component.css']
})
export class ComplianceApplicationDetailsComponent implements OnInit {
  app: ApplicationDetail | null = null;
  eligibilityCheck: EligibilityCheck | null = null; // ADDED: Strictly typed variable

  constructor(
    private route: ActivatedRoute,
    private service: ComplianceOfficerService
  ) { }

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      const appId = +id;

      // Fetch Application Details
      this.service.getApplicationDetails(appId).subscribe({
        next: (res: ApplicationDetail) => {
          console.log(res);
          this.app = res
        },
        error: (err) => console.error("Error loading application details:", err)
      });

      // ADDED: Fetch Eligibility Check Status
      this.service.getLatestEligibilityCheck(appId).subscribe({
        next: (res: EligibilityCheck) => this.eligibilityCheck = res,
        error: (err) => console.error("Error loading eligibility check:", err)
      });
    }
  }

  // Document Viewing Logic
  viewDocument(documentId: number) {
    if (!documentId) return;

    this.service.getDocumentFile(documentId).subscribe({
      next: (blob: Blob) => {
        // Create a temporary, secure URL for the downloaded file in memory
        const fileUrl = window.URL.createObjectURL(blob);

        // Open the file in a new tab
        window.open(fileUrl, '_blank');

        // Optional but good practice: clean up the temporary URL after 10 seconds
        setTimeout(() => {
          window.URL.revokeObjectURL(fileUrl);
        }, 10000);
      },
      error: (err) => {
        console.error("Document fetch failed", err);
        alert('Failed to open document. It may be missing or corrupt.');
      }
    });
  }
}