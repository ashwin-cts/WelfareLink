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
      this.currentUserId = Number(payload.UserId || payload.sub || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier']);
      
      console.log("🚀 Extracted User ID from Token:", this.currentUserId);
      
      this.loadDashboard();
      this.loadProfile();
    } else {
      console.error("❌ No token found in localStorage!");
    }
  }

  loadDashboard() {
    console.log(`📡 Sending request to Dashboard API for ID: ${this.currentUserId}...`);
    this.citizenService.getDashboardStats(this.currentUserId).subscribe({
      next: (data: CitizenDashboardStats) => {
        console.log("✅ DASHBOARD DATA RECEIVED:", data);
        this.stats = data;
      },
      error: (err) => {
        console.error("❌ DASHBOARD API 404 ERROR: The URL is incorrect.");
        console.error(err);
      }
    });
  }

  loadProfile() {
    console.log(`📡 Sending request to Profile API for ID: ${this.currentUserId}...`);
    this.citizenService.getProfile(this.currentUserId).subscribe({
      next: (data: CitizenProfile) => {
        console.log("✅ PROFILE DATA RECEIVED:", data);
        this.profile = data;
      },
      error: (err) => {
        console.error("❌ PROFILE API 404 ERROR: The URL is incorrect.");
        console.error(err);
      }
    });
  }
}