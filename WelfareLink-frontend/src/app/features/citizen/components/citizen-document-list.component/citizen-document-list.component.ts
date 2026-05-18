import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CitizenNavbarComponent } from '../citizen-navbar.component/citizen-navbar.component';
import { CitizenService } from '../../services/citizen.service';
import { CitizenDocument } from '../../models/citizen.model';

@Component({
  selector: 'app-citizen-document-list',
  standalone: true,
  imports: [CommonModule, RouterModule, CitizenNavbarComponent],
  templateUrl: './citizen-document-list.component.html'
})
export class CitizenDocumentListComponent implements OnInit {
  documents: CitizenDocument[] = [];
  filteredDocuments: CitizenDocument[] = [];
  currentFilter: string = '';
  
  tokenUserId!: number;
  actualCitizenId!: number;
  
  isLoading = true;
  successMessage = '';
  errorMessage = '';

  constructor(private citizenService: CitizenService) {}

  ngOnInit(): void {
    const token = localStorage.getItem('token');
    if (token) {
      const payload = JSON.parse(atob(token.split('.')[1]));
      this.tokenUserId = Number(payload.UserId || payload.sub || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier']);
      
      // Fetch the real CitizenId before loading the documents
      this.citizenService.getProfile(this.tokenUserId).subscribe({
          next: (profile) => {
              this.actualCitizenId = profile.citizenId ?? 0;
              this.loadDocuments(); // Now we fetch documents using the correct ID
          },
          error: () => {
              this.errorMessage = "Failed to load profile. Cannot fetch documents.";
              this.isLoading = false;
          }
      });
    }
  }

  loadDocuments() {
    this.isLoading = true;
    this.citizenService.getDocuments(this.actualCitizenId).subscribe({
      next: (data) => {
        this.documents = data;
        this.applyFilter(this.currentFilter);
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load documents.';
        this.isLoading = false;
      }
    });
  }

  applyFilter(status: string) {
    this.currentFilter = status;
    if (!status) {
      this.filteredDocuments = this.documents;
    } else {
      this.filteredDocuments = this.documents.filter(d => d.verificationStatus === status);
    }
  }

  viewDocument(documentId: number) {
    this.citizenService.getDocumentFile(documentId).subscribe({
      next: (blob: Blob) => {
        // Create a temporary, secure URL for the downloaded file in memory
        const fileUrl = window.URL.createObjectURL(blob);
        
        // Open the file in a new tab
        window.open(fileUrl, '_blank');
        
        // Optional but good practice: clean up the temporary URL after a few seconds
        setTimeout(() => {
          window.URL.revokeObjectURL(fileUrl);
        }, 10000);
      },
      error: (err) => {
        let backendError = 'Failed to open document. It may be missing or corrupt.';
        if(err.error && err.error instanceof Blob) {
           // If the backend sent a JSON error inside a blob, it requires parsing, 
           // but a generic message is usually enough here.
           console.error("Document fetch failed", err);
        }
        this.errorMessage = backendError;
      }
    });
  }

  deleteDocument(docId: number) {
    if (confirm('Are you sure you want to delete this document?')) {
      this.citizenService.deleteDocument(docId).subscribe({
        next: () => {
          this.successMessage = 'Document deleted successfully.';
          this.loadDocuments(); 
        },
        error: (err) => {
           let backendError = 'Failed to delete document.';
           if(err.error) {
              if(typeof err.error === 'string') backendError = err.error;
              else if (err.error.Error) backendError = err.error.Error;
              else if (err.error.message) backendError = err.error.message;
           }
           this.errorMessage = backendError;
        }
      });
    }
  }
}