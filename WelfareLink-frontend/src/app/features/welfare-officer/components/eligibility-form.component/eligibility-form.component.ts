import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';

// Make sure your service imports are correct for your folder structure
import { WelfareOfficerService } from '../../services/welfare-officer.services';
import { WelfareApplicationNavbarComponent } from '../welfare-application-navbar.component/welfare-application-navbar.component';

@Component({
  selector: 'app-eligibility-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, WelfareApplicationNavbarComponent],
  templateUrl: './eligibility-form.component.html',
  styleUrls: ['./eligibility-form.component.css']
})
export class EligibilityFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private welfareService = inject(WelfareOfficerService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  eligibilityForm!: FormGroup;
  isEditMode = false;
  currentCheckId: number | null = null;

  // Data containers for the UI
  applicationData: any = null;
  citizenData: any = null;
  documents: any[] = [];

  isLoading = false;
  isSaving = false;
  errorMessage = '';

  ngOnInit(): void {
    this.initForm();

    // Check for Edit Mode (Path param like /eligibility-edit/5)
    this.route.paramMap.subscribe(params => {
      const idParam = params.get('id');

      if (idParam) {
        this.isEditMode = true;
        this.currentCheckId = Number(idParam);
        this.loadEligibilityCheck(this.currentCheckId);
      } else {
        this.isEditMode = false;

        // Use subscribe to safely catch the queryParam
        this.route.queryParams.subscribe(qParams => {
          if (qParams['applicationId']) {
            const appId = Number(qParams['applicationId']);

            // Patch the form and trigger the API call!
            this.eligibilityForm.patchValue({ applicationId: appId });
            this.loadAllRelatedData(appId);
          }
        });

        this.prefillOfficerId();
      }
    });
  }

  initForm() {
    this.eligibilityForm = this.fb.group({
      applicationId: ['', Validators.required],
      officerID: ['', Validators.required],
      result: ['', Validators.required],
      resultCode: ['', Validators.required],
      notes: ['', Validators.required]
    });
  }

  // Called when the user manually types an ID and clicks "Fetch" or loses focus
  onAppIdChange() {
    const appId = this.eligibilityForm.get('applicationId')?.value;
    if (appId && !this.isEditMode) {
      this.loadAllRelatedData(appId);
    }
  }
  loadAllRelatedData(appId: number) {
    this.isLoading = true;

    this.welfareService.getApplicationInfoForCheck(appId).subscribe({
      next: (data: any) => {
        this.applicationData = data.application || data;
        this.citizenData = data.citizen || null;

        // 1. Safely extract ONLY the documents mapped to this specific application.
        // We prioritize the mapping tables FIRST.
        const mappedDocs = this.applicationData?.welfareApplicationDocuments ||
          this.applicationData?.applicationDocuments ||
          data.welfareApplicationDocuments ||
          data.applicationDocuments;

        let rawDocs = [];

        if (mappedDocs && mappedDocs.length > 0) {
          // We found the mapping table! Extract the actual citizen documents from it.
          rawDocs = mappedDocs.map((ad: any) => ad.citizenDocument || ad.document || ad);
        } else if (data.documents) {
          // Fallback: If the backend sent a generic "documents" list, attempt to filter
          // it so we ONLY see documents attached to THIS application ID.
          const filteredDocs = data.documents.filter((d: any) =>
            d.applicationId === appId ||
            d.ApplicationID === appId ||
            d.welfareApplicationId === appId
          );

          // Use filtered docs if we found any, otherwise fallback to all (last resort)
          rawDocs = filteredDocs.length > 0 ? filteredDocs : data.documents;
        }

        // 2. Normalize all the C# properties so the HTML template can easily read them!
        this.documents = rawDocs.map((doc: any) => ({
          ...doc,
          verificationStatus: doc.verificationStatus || doc.VerificationStatus || 'Pending',
          documentID: doc.documentID || doc.DocumentID || doc.id,
          docType: doc.docType || doc.DocType || doc.documentName || 'Unknown Document'
        }));

        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = "Could not find application details for ID #" + appId;
        this.isLoading = false;
      }
    });
  }

  // Ported from Create.cshtml logic 
  updateDocStatus(docId: number, status: string) {
    if (!confirm(`Mark document #${docId} as ${status}?`)) return;

    this.welfareService.updateDocumentStatus(docId, status).subscribe({
      next: () => {
        // Success! Update the UI instantly
        const doc = this.documents.find(d => d.documentID === docId);
        if (doc) doc.verificationStatus = status;
      },
      error: (err) => {
        console.error('Document update error:', err);
        alert("Failed to update document status. Check the console for details.");
      }
    });
  }

  prefillOfficerId() {
    // Attempt to extract Officer ID from the auth token or local storage
    const token = localStorage.getItem('token') || localStorage.getItem('jwt');
    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        const officerId = Number(payload.UserId || payload.sub || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier']);
        this.eligibilityForm.patchValue({ officerID: officerId });
      } catch (e) {
        console.warn('Could not parse Officer ID from token.');
      }
    }
  }

  loadEligibilityCheck(id: number) {
    this.isLoading = true;
    this.welfareService.getEligibilityCheckById(id).subscribe({
      next: (data: any) => {
        const check = data.eligibilityCheck || data;

        this.eligibilityForm.patchValue({
          applicationId: check.applicationId || check.applicationID,
          officerID: check.officerID,
          result: check.result,
          resultCode: check.resultCode,
          notes: check.notes
        });

        // Also fetch the application preview data for the view
        const appId = check.applicationId || check.applicationID;
        if (appId) {
          this.loadAllRelatedData(appId);
        } else {
          this.isLoading = false;
        }
      },
      error: (err) => {
        this.errorMessage = 'Failed to load eligibility check details.';
        this.isLoading = false;
      }
    });
  }
  // --- ADD THIS METHOD TO YOUR TS FILE ---
  onViewDocument(documentId: number) {
    // Make sure your welfareService has this method returning responseType: 'blob'
    this.welfareService.downloadSecureFile(documentId).subscribe({
      next: (blob: Blob) => {
        // Create a local secure URL for the raw file data
        const fileUrl = window.URL.createObjectURL(blob);
        // Open it in a new tab safely
        window.open(fileUrl, '_blank');
      },
      error: (err) => {
        console.error("Failed to download file", err);
        alert("Failed to load document. It may be missing or you may not have permission.");
      }
    });
  }
  onSubmit() {
    if (this.eligibilityForm.invalid) {
      this.eligibilityForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';

    // Format the date strictly as "YYYY-MM-DD"
    const today = new Date().toISOString().split('T')[0];

    // THE FIX: We ensure the ID is explicitly cast to a Number.
    // We also provide both common C# casings (checkID and checkId) 
    // so the .NET Model Binder cannot possibly miss it.
    const checkIdNumber = this.currentCheckId ? Number(this.currentCheckId) : 0;

    const formData = {
      checkID: checkIdNumber,
      checkId: checkIdNumber, // Fallback for ASP.NET Core camelCase binder
      applicationID: Number(this.eligibilityForm.value.applicationId),
      applicationId: Number(this.eligibilityForm.value.applicationId), // Fallback 
      officerID: Number(this.eligibilityForm.value.officerID),
      result: this.eligibilityForm.value.result,
      resultCode: this.eligibilityForm.value.resultCode,
      notes: this.eligibilityForm.value.notes,
      date: today
    };

    if (this.isEditMode && this.currentCheckId) {
      // --- PUT (EDIT MODE) ---
      this.welfareService.updateEligibilityCheck(this.currentCheckId, formData as any).subscribe({
        next: () => this.router.navigate(['/eligibility-list']),
        error: (err) => this.handleError(err)
      });
    } else {
      // --- POST (CREATE MODE) ---
      this.welfareService.createEligibilityCheck(formData as any, formData.applicationID).subscribe({
        next: () => this.router.navigate(['/eligibility-list']),
        error: (err) => this.handleError(err)
      });
    }
  }

  handleError(err: any) {
    this.isSaving = false;
    this.errorMessage = err.error?.message || err.error?.title || 'An error occurred while saving the eligibility check.';
    window.scrollTo({ top: 0, behavior: 'smooth' });
    console.error(err);
  }
}