import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs'; // <-- 1. Add this import!

import { ProgramManagerService } from '../../services/program-manager.service';
import { Resource } from '../../models/program.model';
import { ResourceNavbarComponent } from '../resource-navbar.component/resource-navbar.component';

@Component({
  selector: 'app-resource-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, ResourceNavbarComponent],
  templateUrl: './resource-list.component.html',
  styleUrls: ['./resource-list.component.css']
})
export class ResourceListComponent implements OnInit {
  private programService = inject(ProgramManagerService);

  resources: Resource[] = [];
  filteredResources: Resource[] = [];
  searchTerm: string = '';
  isLoading: boolean = true;

  ngOnInit(): void {
    this.loadResources();
  }

  loadResources() {
    this.isLoading = true;

    // 2. Fetch BOTH Programs and Resources simultaneously
    forkJoin({
      resources: this.programService.getResources(),
      programs: this.programService.getPrograms()
    }).subscribe({
      next: (data) => {
        // 3. Map through the resources and match the title using the programID
        const mappedResources = data.resources.map(res => {
          const matchingProgram = data.programs.find(p => p.programID === res.programID);
          return {
            ...res,
            // Create a guaranteed 'programTitle' property
            programTitle: matchingProgram ? matchingProgram.title : 'Unknown Programme'
          };
        });

        this.resources = mappedResources;
        this.filteredResources = mappedResources;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to load data', err);
        this.isLoading = false;
      }
    });
  }

  onSearch() {
    const term = this.searchTerm.toLowerCase();
    this.filteredResources = this.resources.filter(r =>
      r.resourceID.toString().includes(term) ||
      // Now we can search safely by our new mapped property
      (r.programTitle || '').toLowerCase().includes(term) ||
      r.type.toLowerCase().includes(term)
    );
  }
}