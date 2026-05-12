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
  
  tokenUserId!: number;
  actualCitizenId!: number;
  
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
    
    // Fetch the true CitizenId using the UserId from the token
    if (this.tokenUserId) {
        this.citizenService.getProfile(this.tokenUserId).subscribe({
            next: (profile) => {
                this.actualCitizenId = profile.citizenId ?? 0; 
            },
            error: () => {
                this.errorMessage = "Failed to load your citizen profile. Please try logging in again.";
            }
        });
    }
    
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
      this.tokenUserId = Number(payload.UserId || payload.sub || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier']);
    }
  }

  initForm() {
    this.uploadForm = this.fb.group({
      docType: [{ value: this.preselectedType, disabled: !!this.reuploadId }, Validators.required],
      documentName: ['', Validators.required]
      // Remarks removed!
    });
  }

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      if (file.size > 10 * 1024 * 1024) {
        this.errorMessage = 'File size exceeds 10MB limit.';
        this.selectedFile = null;
        event.target.value = ''; 
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
    
    // Safety check: Make sure the API call to get the CitizenId finished
    if (!this.actualCitizenId) {
        this.errorMessage = "Still synchronizing your profile data. Please wait a second and click upload again.";
        return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';

    if (this.reuploadId) {
      const reuploadData = new FormData();
      reuploadData.append('file', this.selectedFile); 

      this.citizenService.reuploadDocument(this.reuploadId, reuploadData).subscribe({
        next: () => this.router.navigate(['/citizen/documents']),
        error: (err) => {
          this.errorMessage = err.error?.Error || err.error?.message || 'Re-upload failed.';
          this.isSubmitting = false;
        }
      });
    } else {
      const formData = new FormData();
      formData.append('CitizenId', this.actualCitizenId.toString()); 
      formData.append('DocType', this.uploadForm.get('docType')?.value);
      formData.append('DocumentName', this.uploadForm.get('documentName')?.value);
      // Remarks append removed!
      formData.append('file', this.selectedFile); 

      this.citizenService.uploadDocument(formData).subscribe({
        next: () => this.router.navigate(['/citizen/documents']),
        error: (err) => {
          let backendError = 'Upload failed due to a server error.';
          if(err.error) {
             if(typeof err.error === 'string') backendError = err.error;
             else if (err.error.Error) backendError = err.error.Error;
             else if (err.error.message) backendError = err.error.message;
          }
          this.errorMessage = backendError;
          this.isSubmitting = false;
        }
      });
    }
  }
}