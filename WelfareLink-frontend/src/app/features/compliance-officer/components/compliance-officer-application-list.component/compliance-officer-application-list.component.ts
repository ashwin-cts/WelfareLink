import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { ComplianceOfficerService } from '../../services/compliance-officer.service'; 
import { ApplicationDetail, EligibilityCheck } from '../../models/compliance-officer.model';

@Component({
  selector: 'app-compliance-application-details',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './compliance-officer-application-list.component.html',
  styleUrls: ['./compliance-officer-application-list.component.css']
})
export class ComplianceApplicationDetailsComponent implements OnInit {
  app: ApplicationDetail | any = null;
  eligibilityCheck: EligibilityCheck | null = null; 

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
        next: (res: any) => {
          console.log(res);
          this.app = res;
          
          // EXTRACT LATEST STATUS: Find the eligibility check with the highest checkID
          if (res.eligibilityChecks && res.eligibilityChecks.length > 0) {
            this.eligibilityCheck = res.eligibilityChecks.reduce((prev: any, current: any) => 
              (prev.checkID > current.checkID) ? prev : current
            );
          }
        },
        error: (err) => console.error("Error loading application details:", err)
      });
    }
  }

  // Document Viewing Logic
  viewDocument(documentId: number) {
    if (!documentId) return;

    this.service.getDocumentFile(documentId).subscribe({
      next: (blob: Blob) => {
        const fileUrl = window.URL.createObjectURL(blob);
        window.open(fileUrl, '_blank');

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