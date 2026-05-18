import { Component, OnInit, inject, signal, ChangeDetectorRef } from '@angular/core'; // Added signal and ChangeDetectorRef
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { ProgramManagerService } from '../../services/program-manager.service';
import { WelfareProgram, Resource } from '../../models/program.model';
import { ProgramManagerNavbarComponent } from '../program-manager-navbar.component/program-manager-navbar.component';

@Component({
  selector: 'app-program-details',
  standalone: true,
  imports: [CommonModule, RouterModule, ProgramManagerNavbarComponent],
  templateUrl: './program-details.component.html',
  styleUrls: ['./program-details.component.css']
})
export class ProgramDetailsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private programService = inject(ProgramManagerService);
  private cdr = inject(ChangeDetectorRef); // Essential for forcing UI updates

  // Using a Signal makes the UI highly reactive
  program = signal<WelfareProgram | null>(null);
  resources = signal<Resource[]>([]);
  utilisationPercentage = signal<number>(0);
  remainingBudget = signal<number>(0);
  isLoading = signal<boolean>(true);
  totalAllocatedFunds = signal<number>(0);
  applicationCount = signal<number>(0);

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) {
      this.loadAllDetails(id);
    }
  }

  loadAllDetails(id: number) {
    this.isLoading.set(true);
    this.programService.getProgramById(id).subscribe({
      next: (data: any) => {
        // Extract the nested 'program' object from the API wrapper
        this.program.set(data.program);
        this.resources.set(data.resources || []); // Update the resources signal here
        // Extract the summary stats from the API wrapper
        this.utilisationPercentage.set(data.utilisationPercentage || 0);
        this.remainingBudget.set(data.remainingBudget || 0);
        this.totalAllocatedFunds.set(data.totalAllocatedFunds || 0);
        this.isLoading.set(false);
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error(err);
        this.isLoading.set(false);
      }
    });
  }

  loadResources(programId: number) {
    this.programService.getResourcesByProgram(programId).subscribe({
      next: (data) => {
        this.resources.set(data);
        this.isLoading.set(false);
        this.cdr.detectChanges();
      },
      error: () => {
        this.isLoading.set(false);
        this.cdr.detectChanges();
      }
    });
  }
}