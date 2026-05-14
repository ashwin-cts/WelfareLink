import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { ComplianceOfficerService } from '../../services/compliance-officer.service';
import { ApplicationDetail } from '../../models/compliance-officer.model';

@Component({
  selector: 'app-compliance-application-details',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div *ngIf="app; else loadingBlock">
      
      <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h2 class="h4 mb-1"><i class="bi bi-file-earmark-text text-primary me-2"></i>Compliance View - Application #{{ app.applicationID }}</h2>
          <p class="text-muted mb-0">Detailed view for compliance investigations</p>
        </div>
        <div>
          <button class="btn btn-outline-secondary shadow-sm" routerLink="/compliance/dashboard">
            <i class="bi bi-arrow-left me-1"></i> Back to Dashboard
          </button>
        </div>
      </div>

      <div class="row">
        <div class="col-md-8">
          
          <div class="card shadow-sm border-0 mb-4">
            <div class="card-body">
              <h5 class="card-title text-primary border-bottom pb-2 mb-3">Application Information</h5>
              
              <div class="row g-3 mb-4">
                <div class="col-sm-4">
                  <label class="text-muted small fw-bold text-uppercase">Application ID</label>
                  <p class="mb-0 fw-medium">#{{ app.applicationID }}</p>
                </div>
                <div class="col-sm-4">
                  <label class="text-muted small fw-bold text-uppercase">Citizen ID</label>
                  <p class="mb-0 fw-medium">#{{ app.citizenID }}</p>
                </div>
                <div class="col-sm-4">
                  <label class="text-muted small fw-bold text-uppercase">Citizen Name</label>
                  <p class="mb-0 fw-medium">{{ app.citizen?.name || '-' }}</p>
                </div>
                <div class="col-sm-4">
                  <label class="text-muted small fw-bold text-uppercase">Program Name</label>
                  <p class="mb-0 fw-medium">{{ app.program?.title || '-' }}</p>
                </div>
                <div class="col-sm-4">
                  <label class="text-muted small fw-bold text-uppercase">Submitted Date</label>
                  <p class="mb-0 fw-medium">{{ app.submittedDate | date:'mediumDate' }}</p>
                </div>
                <div class="col-sm-4">
                  <label class="text-muted small fw-bold text-uppercase">Status</label>
                  <p class="mb-0">
                    <span class="badge bg-primary">{{ app.status || '-' }}</span>
                  </p>
                </div>
              </div>

              <h6 class="fw-bold mb-3 text-secondary">Application Documents</h6>
              <div class="table-responsive" *ngIf="app.applicationDocuments && app.applicationDocuments.length > 0; else noDocs">
                <table class="table table-sm table-bordered mb-0">
                  <thead class="table-light">
                    <tr>
                      <th>Document Type</th>
                      <th>Name</th>
                      <th>Uploaded Date</th>
                      <th style="width: 100px;">Action</th> </tr>
                  </thead>
                  <tbody>
                    <tr *ngFor="let docLink of app.applicationDocuments">
                      <ng-container *ngIf="docLink.citizenDocument as doc">
                        <td><span class="badge bg-info text-dark">{{ doc.docType || 'Document' }}</span></td>
                        <td>{{ doc.documentName || '-' }}</td>
                        <td>{{ doc.uploadedDate | date:'mediumDate' }}</td>
                        <td>
                          <button class="btn btn-sm btn-outline-primary w-100" (click)="viewDocument(doc.documentID)">
                            <i class="bi bi-eye"></i> View
                          </button>
                        </td>
                      </ng-container>
                    </tr>
                  </tbody>
                </table>
              </div>
              <ng-template #noDocs><p class="text-muted fst-italic">No documents uploaded.</p></ng-template>
            </div>
          </div>

          <div class="card shadow-sm border-0 mb-4">
            <div class="card-body">
              <h5 class="card-title text-success border-bottom pb-2 mb-3">Benefits & Disbursements</h5>
              
              <div *ngIf="app.benefits && app.benefits.length > 0; else noBenefits">
                <div *ngFor="let b of app.benefits" class="p-3 border rounded bg-light mb-3">
                  <div class="d-flex justify-content-between align-items-center mb-2">
                    <h6 class="mb-0 text-dark fw-bold">Benefit #{{ b.benefitID }} - {{ b.type }}</h6>
                    <span class="badge bg-success">{{ b.status }}</span>
                  </div>
                  <div class="row mb-3">
                    <div class="col-6"><p class="mb-0 small"><strong>Amount:</strong> {{ b.amount | currency:'INR' }}</p></div>
                    <div class="col-6"><p class="mb-0 small text-end"><strong>Date:</strong> {{ b.date | date:'mediumDate' }}</p></div>
                  </div>

                  <h6 class="fw-bold text-secondary mb-2 mt-3 small text-uppercase">Disbursements List</h6>
                  <div *ngIf="b.disbursements && b.disbursements.length > 0; else noDisb">
                    <table class="table table-sm bg-white mb-0">
                      <thead>
                        <tr>
                          <th>ID</th>
                          <th>Date</th>
                          <th>Amount</th>
                          <th>Status</th>
                        </tr>
                      </thead>
                      <tbody>
                        <tr *ngFor="let d of b.disbursements">
                          <td>#{{ d.disbursementID }}</td>
                          <td>{{ d.date | date:'mediumDate' }}</td>
                          <td class="fw-medium text-success">{{ d.amount | currency:'INR' }}</td>
                          <td><span class="badge bg-secondary">{{ d.status }}</span></td>
                        </tr>
                      </tbody>
                    </table>
                  </div>
                  <ng-template #noDisb><p class="text-muted small fst-italic mb-0">No disbursements processed yet.</p></ng-template>
                </div>
              </div>
              <ng-template #noBenefits><p class="text-muted fst-italic">No benefits allocated.</p></ng-template>
            </div>
          </div>

        </div>

        <div class="col-md-4">
          <div class="card shadow-sm border-0 mb-4 border-top border-danger border-4">
            <div class="card-body text-center">
              <h5 class="mb-3 text-danger"><i class="bi bi-shield-lock me-2"></i>Quick Actions</h5>
              <p class="text-muted small mb-4">Investigate and flag any compliance violations related to this application, its benefits, or disbursements.</p>
              
              <div class="d-grid gap-2">
                <button class="btn btn-danger shadow-sm" [routerLink]="['/compliance/flag-issue', app.applicationID]">
                  <i class="bi bi-flag-fill me-2"></i> Raise Compliance Flag
                </button>
              </div>
            </div>
          </div>

          <div class="card shadow-sm border-0 bg-light">
            <div class="card-body">
              <h6 class="fw-bold"><i class="bi bi-info-circle me-2"></i>Officer Notes</h6>
              <p class="text-muted small mb-0">
                Use this view to cross-reference application data against allocated benefits. Discrepancies between Max Benefit and Allocated Amounts should be flagged immediately.
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <ng-template #loadingBlock>
      <div class="text-center py-5">
        <div class="spinner-border text-primary" role="status"></div>
        <p class="mt-2 text-muted">Loading Application Data...</p>
      </div>
    </ng-template>
  `
})
export class ComplianceApplicationDetailsComponent implements OnInit {
  app: ApplicationDetail | null = null;

  constructor(
    private route: ActivatedRoute, 
    private service: ComplianceOfficerService
  ) {}

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.service.getApplicationDetails(+id).subscribe({
        next: (res: ApplicationDetail) => this.app = res,
        error: (err) => console.error("Error loading application details:", err)
      });
    }
  }

  // ADDED: Document Viewing Logic
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