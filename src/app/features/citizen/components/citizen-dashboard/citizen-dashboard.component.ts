import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CitizenNavbarComponent } from '../citizen-navbar.component/citizen-navbar.component';
import { CitizenService } from '../../services/citizen.service';
import { CitizenDashboardStats, CitizenProfile } from '../../models/citizen.model';

@Component({
  selector: 'app-citizen-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, CitizenNavbarComponent],
  templateUrl: './citizen-dashboard.component.html'
})
export class CitizenDashboardComponent implements OnInit {
  stats: CitizenDashboardStats | null = null;
  profile: CitizenProfile | null = null; 
  currentUserId!: number;

  constructor(private citizenService: CitizenService) {}

  ngOnInit(): void {
    const token = localStorage.getItem('token');
    if (token) {
      const payload = JSON.parse(atob(token.split('.')[1]));
      // 1. Get the User ID from the Token
      this.currentUserId = Number(payload.UserId || payload.sub || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier']);
      
      console.log("🚀 Token UserId:", this.currentUserId);
      
      // 2. Fetch the profile FIRST to discover the CitizenId
      this.loadProfile();
    }
  }

  loadProfile() {
    console.log(`📡 Fetching Profile for UserId: ${this.currentUserId}...`);
    
    this.citizenService.getProfile(this.currentUserId).subscribe({
      next: (data: CitizenProfile) => {
        this.profile = data;
        console.log("✅ PROFILE DATA RECEIVED:", data);

        // 3. NOW that we have the real CitizenId, fetch the dashboard stats!
        if (data.citizenId) {
            this.loadDashboard(data.citizenId);
        } else {
            console.error("❌ Profile loaded, but no CitizenId was found in the data!");
        }
      },
      error: (err) => console.error("❌ PROFILE 404 ERROR:", err)
    });
  }

  // Notice this now requires a citizenId, not a userId!
  loadDashboard(actualCitizenId: number) {
    console.log(`📡 Fetching Dashboard Stats for CitizenId: ${actualCitizenId}...`);
    
    this.citizenService.getDashboardStats(actualCitizenId).subscribe({
      next: (data: CitizenDashboardStats) => {
        this.stats = data;
        console.log("✅ DASHBOARD DATA RECEIVED:", data);
      },
      error: (err) => console.error("❌ DASHBOARD 404 ERROR:", err)
    });
  }
}