import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CitizenNavbarComponent } from '../citizen-navbar.component/citizen-navbar.component';
import { CitizenService } from '../../services/citizen.service';
import { WelfareApplication } from '../../../Gov-auditor/models/auditor.model'; // Shared model

@Component({
  selector: 'app-citizen-applications',
  standalone: true,
  imports: [CommonModule, RouterModule, CitizenNavbarComponent],
  templateUrl: './citizen-applications.component.html'
})
export class CitizenApplicationsComponent implements OnInit {
  applications: WelfareApplication[] = [];
  currentUserId!: number;
  isLoading = true;
  errorMessage = '';

  constructor(private citizenService: CitizenService) {}

  ngOnInit(): void {
    const token = localStorage.getItem('token');
    if (token) {
      const payload = JSON.parse(atob(token.split('.')[1]));
      this.currentUserId = Number(payload.UserId || payload.sub || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier']);
      this.loadApplications();
    }
  }

  loadApplications() {
    this.isLoading = true;
    this.citizenService.getApplications(this.currentUserId).subscribe({
      next: (data) => {
        this.applications = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error(err);
        this.errorMessage = 'Failed to load applications.';
        this.isLoading = false;
      }
    });
  }
}