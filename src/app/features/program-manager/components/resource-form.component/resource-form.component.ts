import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { forkJoin } from 'rxjs';

import { ProgramManagerService } from '../../services/program-manager.service';
import { WelfareProgram, Resource } from '../../models/program.model';
import { ResourceNavbarComponent } from '../resource-navbar.component/resource-navbar.component';

@Component({
  selector: 'app-resource-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, ResourceNavbarComponent],
  templateUrl: './resource-form.component.html',
  styleUrls: ['./resource-form.component.css']
})
export class ResourceFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private programService = inject(ProgramManagerService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  resourceForm!: FormGroup;
  isEditMode = false;
  currentResourceId: number | null = null;

  programs: WelfareProgram[] = [];
  selectedProgramDetails: any = null; // Holds the budget info

  isLoading = false;
  isSaving = false;
  errorMessage = '';

  ngOnInit(): void {
    this.initForm();
    this.loadPrograms();

    // Check if we are Editing an existing resource
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.isEditMode = true;
      this.currentResourceId = Number(idParam);
      this.loadResourceData(this.currentResourceId);
    } else {
      // Check if we came from a specific program dashboard (Query Param)
      this.route.queryParams.subscribe(params => {
        if (params['programId']) {
          this.resourceForm.patchValue({ programID: Number(params['programId']) });
        }
      });
    }

    // Listen to changes on the Program Dropdown to fetch budget stats
    this.resourceForm.get('programID')?.valueChanges.subscribe(programId => {
      if (programId && !this.isEditMode) {
        this.fetchProgramBudgetDetails(Number(programId));
      }
    });
  }

  initForm() {
    this.resourceForm = this.fb.group({
      programID: ['', Validators.required],
      type: ['', Validators.required],
      quantity: [null, [Validators.required, Validators.min(0.01)]],
      status: ['Available'] // Default for Create mode
    });
  }

  loadPrograms() {
    this.programService.getPrograms().subscribe(data => {
      this.programs = data;
    });
  }

  fetchProgramBudgetDetails(programId: number) {
    this.programService.getProgramById(programId).subscribe((data: any) => {
      const p = data.program || data;
      const budget = p.budget || 0;
      const allocated = data.totalAllocatedFunds || 0;

      this.selectedProgramDetails = {
        title: p.title,
        budget: budget,
        allocated: allocated,
        remaining: budget - allocated
      };
    });
  }

  loadResourceData(id: number) {
    this.isLoading = true;

    // Since there isn't a strict GET /api/ResourceApi/{id} endpoint in the Swagger, 
    // we fetch all resources and find the specific one to edit.
    this.programService.getResources().subscribe({
      next: (resources) => {
        const resourceToEdit = resources.find(r => r.resourceID === id);

        if (resourceToEdit) {
          this.resourceForm.patchValue(resourceToEdit);
          // Manually fetch the program details to show the fixed name
          this.fetchProgramBudgetDetails(resourceToEdit.programID);
        } else {
          this.errorMessage = "Resource not found!";
        }
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load resource details.';
        this.isLoading = false;
      }
    });
  }

  onSubmit() {
    if (this.resourceForm.invalid) {
      this.resourceForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';
    const formData: Resource = this.resourceForm.getRawValue(); // gets values even if disabled

    if (this.isEditMode && this.currentResourceId) {
      formData.resourceID = this.currentResourceId;

      this.programService.updateResource(this.currentResourceId, formData).subscribe({
        next: () => this.router.navigate(['/resource-manager']),
        error: (err) => this.handleError(err)
      });
    } else {
      this.programService.addResource(formData).subscribe({
        next: () => this.router.navigate(['/resource-manager']),
        error: (err) => this.handleError(err)
      });
    }
  }

  handleError(err: any) {
    this.isSaving = false;
    this.errorMessage = err.error?.message || err.error?.title || 'An error occurred while saving.';
    console.error(err);
  }
}