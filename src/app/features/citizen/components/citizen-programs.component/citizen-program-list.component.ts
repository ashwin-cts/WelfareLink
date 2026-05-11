import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { forkJoin } from 'rxjs';
import { CitizenNavbarComponent } from '../citizen-navbar.component/citizen-navbar.component';
import { CitizenService } from '../../services/citizen.service';
import { WelfareProgram } from '../../models/citizen.model';

@Component({
  selector: 'app-citizen-program-list',
  standalone: true,
  imports: [CommonModule, RouterModule, CitizenNavbarComponent],
  templateUrl: './citizen-program-list.component.html',
  styleUrls: ['./citizen-program-list.component.css']
})
export class CitizenProgramListComponent implements OnInit {
  programs: WelfareProgram[] = [];
  appliedProgramIds = new Set<number>();
  citizenGender: string = '';
  currentUserId!: number;
  
  isLoading = true;
  selectedProgram: WelfareProgram | null = null; // For the details modal

  constructor(private citizenService: CitizenService) {}

  ngOnInit(): void {
    const token = localStorage.getItem('token');
    if (token) {
      const payload = JSON.parse(atob(token.split('.')[1]));
      this.currentUserId = Number(payload.UserId || payload.sub || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier']);
      this.loadInitialData();
    }
  }

  loadInitialData() {
    this.isLoading = true;
    
    // Fetch Programs, Applications (to find applied IDs), and Profile (for gender) all at once!
    forkJoin({
      programs: this.citizenService.getPrograms(),
      applications: this.citizenService.getApplications(this.currentUserId),
      profile: this.citizenService.getProfile(this.currentUserId)
    }).subscribe({
      next: (res) => {
        this.programs = res.programs;
        this.citizenGender = res.profile.gender || '';
        // Extract the program IDs from the user's applications
        res.applications.forEach(app => this.appliedProgramIds.add(app.programID));
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to load program data', err);
        this.isLoading = false;
      }
    });
  }

  isApplied(programId: number): boolean {
    return this.appliedProgramIds.has(programId);
  }

  isGenderBlocked(eligibleGender?: string): boolean {
    if (!eligibleGender || eligibleGender === 'Anyone') return false;
    const allowedGenders = eligibleGender.split(',').map(g => g.trim().toLowerCase());
    return !allowedGenders.includes(this.citizenGender.toLowerCase());
  }

  openModal(program: WelfareProgram) {
    this.selectedProgram = program;
  }

  closeModal() {
    this.selectedProgram = null;
  }
}