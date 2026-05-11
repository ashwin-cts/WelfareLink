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
  currentUserId!: number;
  isLoading = true;
  successMessage = '';
  errorMessage = '';

  constructor(private citizenService: CitizenService) {}

  ngOnInit(): void {
    const token = localStorage.getItem('token');
    if (token) {
      const payload = JSON.parse(atob(token.split('.')[1]));
      this.currentUserId = Number(payload.UserId || payload.sub || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier']);
      this.loadDocuments();
    }
  }

  loadDocuments() {
    this.isLoading = true;
    this.citizenService.getDocuments(this.currentUserId).subscribe({
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

  deleteDocument(docId: number) {
    if (confirm('Are you sure you want to delete this document?')) {
      this.citizenService.deleteDocument(docId).subscribe({
        next: () => {
          this.successMessage = 'Document deleted successfully.';
          this.loadDocuments(); // Refresh the list
        },
        error: (err) => this.errorMessage = err.error?.message || 'Failed to delete document.'
      });
    }
  }
}