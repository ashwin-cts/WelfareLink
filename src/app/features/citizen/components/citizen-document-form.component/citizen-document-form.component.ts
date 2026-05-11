import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { CitizenNavbarComponent } from '../citizen-navbar.component/citizen-navbar.component';
import { CitizenService } from '../../services/citizen.service';

@Component({
  selector: 'app-citizen-document-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, CitizenNavbarComponent],
  templateUrl: './citizen-document-form.component.html'
})
export class CitizenDocumentFormComponent implements OnInit {
  uploadForm!: FormGroup;
  selectedFile: File | null = null;
  isSubmitting = false;
  errorMessage = '';
  currentUserId!: number;
  
  // Re-upload tracking
  reuploadId: number | null = null;
  preselectedType: string = '';

  constructor(
    private fb: FormBuilder,
    private citizenService: CitizenService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.extractUserIdFromToken();
    
    // Check if we are re-uploading an existing rejected document
    this.route.queryParams.subscribe(params => {
      this.reuploadId = params['reuploadId'] ? Number(params['reuploadId']) : null;
      this.preselectedType = params['type'] || '';
      this.initForm();
    });
  }

  extractUserIdFromToken() {
    const token = localStorage.getItem('token');
    if (token) {
      const payload = JSON.parse(atob(token.split('.')[1]));
      this.currentUserId = Number(payload.UserId || payload.sub || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier']);
    }
  }

  initForm() {
    this.uploadForm = this.fb.group({
      docType: [{ value: this.preselectedType, disabled: !!this.reuploadId }, Validators.required],
      documentName: ['', Validators.required],
      remarks: ['']
    });
  }

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      // Validate size (10MB)
      if (file.size > 10 * 1024 * 1024) {
        this.errorMessage = 'File size exceeds 10MB limit.';
        this.selectedFile = null;
        event.target.value = ''; // Reset input
        return;
      }
      this.selectedFile = file;
      this.errorMessage = '';
    }
  }

  onSubmit() {
    if (this.uploadForm.invalid || !this.selectedFile) {
      this.errorMessage = "Please fill all required fields and select a file.";
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';

    // Create FormData for file upload
    const formData = new FormData();
    formData.append('CitizenId', this.currentUserId.toString());
    formData.append('DocType', this.reuploadId ? this.preselectedType : this.uploadForm.get('docType')?.value);
    formData.append('DocumentName', this.uploadForm.get('documentName')?.value);
    formData.append('Remarks', this.uploadForm.get('remarks')?.value || '');
    formData.append('FileUpload', this.selectedFile);

    if (this.reuploadId) {
      formData.append('DocumentID', this.reuploadId.toString());
      // Call your Reupload endpoint (Assuming your service has an updateDocument method. If not, fallback to upload)
      this.citizenService.uploadDocument(formData).subscribe({
        next: () => this.router.navigate(['/citizen/documents']),
        error: (err) => {
          this.errorMessage = err.error?.message || 'Upload failed.';
          this.isSubmitting = false;
        }
      });
    } else {
      this.citizenService.uploadDocument(formData).subscribe({
        next: () => this.router.navigate(['/citizen/documents']),
        error: (err) => {
          this.errorMessage = err.error?.message || 'Upload failed.';
          this.isSubmitting = false;
        }
      });
    }
  }
}