import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms'; // Required for ngModel search

import { ProgramManagerService } from '../../services/program-manager.service';
import { WelfareProgram } from '../../models/program.model';
import { ProgramManagerNavbarComponent } from '../program-manager-navbar.component/program-manager-navbar.component';

@Component({
  selector: 'app-program-list',
  standalone: true,
  // Make sure FormsModule is in the imports array!
  imports: [CommonModule, RouterModule, FormsModule, ProgramManagerNavbarComponent],
  templateUrl: './program-list.component.html',
  styleUrls: ['./program-list.component.css']
})
export class ProgramListComponent implements OnInit {
  private programService = inject(ProgramManagerService);

  programs: WelfareProgram[] = [];
  filteredPrograms: WelfareProgram[] = []; // Used for the search filter
  searchTerm: string = '';
  isLoading: boolean = true;

  ngOnInit(): void {
    this.loadPrograms();
  }

  loadPrograms() {
    this.isLoading = true;
    this.programService.getPrograms().subscribe({
      next: (data) => {
        this.programs = data;
        this.filteredPrograms = data; // Initialize the table with all data
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to load programs', err);
        this.isLoading = false;
      }
    });
  }

  // This fires every time the user types in the search box
  onSearch() {
    const term = this.searchTerm.toLowerCase();
    this.filteredPrograms = this.programs.filter(p =>
      p.title.toLowerCase().includes(term) ||
      p.programID.toString().includes(term)
    );
  }
}