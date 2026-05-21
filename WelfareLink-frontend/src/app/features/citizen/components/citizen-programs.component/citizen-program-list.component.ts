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

  tokenUserId!: number;
  actualCitizenId!: number;

  isLoading = true;
  selectedProgram: WelfareProgram | null = null;

  constructor(private citizenService: CitizenService) { }

  ngOnInit(): void {
    const token = localStorage.getItem('token');
    if (token) {
      const payload = JSON.parse(atob(token.split('.')[1]));
      this.tokenUserId = Number(payload.UserId || payload.sub || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier']);

      // 1. Fetch Profile First
      this.citizenService.getProfile(this.tokenUserId).subscribe({
        next: (profile) => {
          this.actualCitizenId = profile.citizenId ?? 0;
          this.citizenGender = profile.gender || (profile as any).Gender || '';
          this.loadInitialData(); // 2. Now load the rest
        },
        error: () => {
          this.isLoading = false;
          console.error("Failed to load profile");
        }
      });
    }
  }

  loadInitialData() {
    forkJoin({
      programs: this.citizenService.getPrograms(),
      applications: this.citizenService.getApplications(this.actualCitizenId)
    }).subscribe({
      next: (res) => {
        this.programs = res.programs
          .filter(p => !this.isFutureDate(p.startDate))
          .map(p => {
            // Calculate a readable duration string from startDate and endDate
            const start = new Date(p.startDate).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
            const end = new Date(p.endDate).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });

            return {
              ...p,
              duration: `${start} - ${end}`, // Formats as "20 Apr 2026 - 20 Oct 2026"
              eligibleGender: p.eligibleGender || (p as any).EligibleGender || 'Anyone',
              requiredDocuments: p.requiredDocuments || (p as any).RequiredDocuments || 'None'
            };
          });

        res.applications.forEach(app => this.appliedProgramIds.add(app.programID));
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to load program data', err);
        this.isLoading = false;
      }
    });
  }

  isFutureDate(dateString: string | Date): boolean {
    if (!dateString) return false;

    const programDate = new Date(dateString);
    const today = new Date();

    // Set both times to midnight (00:00:00) so we only compare the actual dates, not the exact hours
    programDate.setHours(0, 0, 0, 0);
    today.setHours(0, 0, 0, 0);

    // Returns true if the program's start date is strictly greater than today
    return programDate.getTime() > today.getTime();
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