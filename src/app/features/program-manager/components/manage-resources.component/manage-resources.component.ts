import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { forkJoin } from 'rxjs';

import { ProgramManagerService } from '../../services/program-manager.service';
import { WelfareProgram, Resource } from '../../models/program.model';
import { ResourceNavbarComponent } from '../resource-navbar.component/resource-navbar.component';

@Component({
  selector: 'app-manage-resources',
  standalone: true,
  imports: [CommonModule, RouterModule, ResourceNavbarComponent],
  templateUrl: './manage-resources.component.html',
  styleUrls: ['./manage-resources.component.css']
})
export class ManageResourcesComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private programService = inject(ProgramManagerService);

  // Signals for reactive, lightning-fast UI updates
  program = signal<WelfareProgram | null>(null);
  resources = signal<Resource[]>([]);

  // KPI Signals
  totalAllocated = signal<number>(0);
  remainingBudget = signal<number>(0);
  utilisationPercentage = signal<number>(0);

  isLoading = signal<boolean>(true);

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) {
      this.loadData(id);
    }
  }

  loadData(programId: number) {
    this.isLoading.set(true);

    // Fetch Program info and Resource List simultaneously
    forkJoin({
      programData: this.programService.getProgramById(programId),
      resourceData: this.programService.getResourcesByProgram(programId)
    }).subscribe({
      next: (result: any) => {
        // Handle potential C# wrappers
        const p = result.programData.program || result.programData;
        this.program.set(p);

        // Handle Resource array
        const resList = result.resourceData.resources || result.resourceData || [];
        this.resources.set(resList);

        // --- THE FIX: Use the exact C# backend calculations! ---
        // We use || to handle both camelCase and PascalCase from the C# serializer
        this.totalAllocated.set(result.programData.totalAllocatedFunds || result.programData.TotalAllocatedFunds || 0);
        this.remainingBudget.set(result.programData.remainingBudget || result.programData.RemainingBudget || 0);
        this.utilisationPercentage.set(result.programData.utilisationPercentage || result.programData.UtilisationPercentage || 0);
        // -------------------------------------------------------

        this.isLoading.set(false);
      },
      error: (err) => {
        console.error("Failed to load program resources", err);
        this.isLoading.set(false);
      }
    });
  }

  getTotalFundsAllocated(): number {
    return this.totalAllocated();
  }
}