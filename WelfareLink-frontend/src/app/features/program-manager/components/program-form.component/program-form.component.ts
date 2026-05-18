import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';

import { ProgramManagerService } from '../../services/program-manager.service';
import { WelfareProgram } from '../../models/program.model';
import { ProgramManagerNavbarComponent } from '../program-manager-navbar.component/program-manager-navbar.component';

@Component({
  selector: 'app-program-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, ProgramManagerNavbarComponent],
  templateUrl: './program-form.component.html',
  styleUrls: ['./program-form.component.css']
})
export class ProgramFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private programService = inject(ProgramManagerService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  programForm!: FormGroup;
  isEditMode = false;
  currentProgramId: number | null = null;

  isLoading = false;
  isSaving = false;
  errorMessage = '';

  // Checkbox State Management
  selectedGenders: string[] = ['Anyone'];
  selectedDocs: string[] = ['None'];

  genderOptions = [
    { value: 'Anyone', label: 'Anyone (No Restriction)', icon: 'bi-people text-success' },
    { value: 'Male', label: 'Male', icon: 'bi-gender-male text-primary' },
    { value: 'Female', label: 'Female', icon: 'bi-gender-female text-danger' },
    { value: 'Other', label: 'Other', icon: 'bi-gender-ambiguous text-warning' }
  ];

  docOptions = [
    { value: 'None', label: 'No Document Required', icon: 'bi-x-circle text-secondary' },
    { value: 'ID Proof', label: 'ID Proof', icon: 'bi-person-vcard text-primary' },
    { value: 'Residence Proof', label: 'Residence Proof', icon: 'bi-house text-success' },
    { value: 'Income Certificate', label: 'Income Certificate', icon: 'bi-currency-rupee text-warning' },
    { value: 'Bank Statement', label: 'Bank Statement', icon: 'bi-bank text-info' }
  ];

  ngOnInit(): void {
    this.initForm();

    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.isEditMode = true;
      this.currentProgramId = Number(idParam);
      this.loadProgramData(this.currentProgramId);
    }
  }

  initForm() {
    this.programForm = this.fb.group({
      title: ['', Validators.required],
      description: ['', Validators.required],
      startDate: ['', Validators.required],
      endDate: ['', Validators.required],
      budget: [null, [Validators.required, Validators.min(0)]],
      maxBenefitPerCitizen: [null, [Validators.required, Validators.min(0)]],
      status: ['Active', Validators.required], // Defaults to Active for Create
      eligibleGender: ['Anyone'],
      requiredDocuments: ['None']
    });
  }

  loadProgramData(id: number) {
    this.isLoading = true;
    this.programService.getProgramById(id).subscribe({
      next: (data: any) => {
        const program = data.program || data;

        // Format dates for the HTML <input type="date">
        const formattedStartDate = program.startDate ? new Date(program.startDate).toISOString().split('T')[0] : '';
        const formattedEndDate = program.endDate ? new Date(program.endDate).toISOString().split('T')[0] : '';

        // Parse existing checkboxes
        if (program.eligibleGender) {
          this.selectedGenders = program.eligibleGender.split(',').map((s: string) => s.trim());
        }
        if (program.requiredDocuments) {
          this.selectedDocs = program.requiredDocuments.split(',').map((s: string) => s.trim());
        }

        this.programForm.patchValue({
          ...program,
          startDate: formattedStartDate,
          endDate: formattedEndDate,
          eligibleGender: this.selectedGenders.join(', '),
          requiredDocuments: this.selectedDocs.join(', ')
        });

        this.isLoading = false;
      },
      error: (err) => {
        this.errorMessage = 'Failed to load program details.';
        this.isLoading = false;
      }
    });
  }

  // --- Checkbox Logic translated from your MVC JavaScript ---
  onGenderChange(value: string, event: any) {
    const isChecked = event.target.checked;

    if (value === 'Anyone' && isChecked) {
      this.selectedGenders = ['Anyone'];
    } else if (isChecked) {
      this.selectedGenders = this.selectedGenders.filter(g => g !== 'Anyone');
      this.selectedGenders.push(value);
    } else {
      this.selectedGenders = this.selectedGenders.filter(g => g !== value);
      if (this.selectedGenders.length === 0) this.selectedGenders = ['Anyone'];
    }
    this.programForm.patchValue({ eligibleGender: this.selectedGenders.join(', ') });
  }

  onDocChange(value: string, event: any) {
    const isChecked = event.target.checked;

    if (value === 'None' && isChecked) {
      this.selectedDocs = ['None'];
    } else if (isChecked) {
      this.selectedDocs = this.selectedDocs.filter(d => d !== 'None');
      this.selectedDocs.push(value);
    } else {
      this.selectedDocs = this.selectedDocs.filter(d => d !== value);
      if (this.selectedDocs.length === 0) this.selectedDocs = ['None'];
    }
    this.programForm.patchValue({ requiredDocuments: this.selectedDocs.join(', ') });
  }

  onSubmit() {
    if (this.programForm.invalid) {
      this.programForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';
    const formData: WelfareProgram = this.programForm.value;

    if (this.isEditMode && this.currentProgramId) {
      formData.programID = this.currentProgramId;
      this.programService.updateProgram(this.currentProgramId, formData).subscribe({
        next: () => this.router.navigate(['/program-manager/list']),
        error: (err) => this.handleError(err)
      });
    } else {
      this.programService.createProgram(formData).subscribe({
        next: () => this.router.navigate(['/program-manager/list']),
        error: (err) => this.handleError(err)
      });
    }
  }

  handleError(err: any) {
    this.isSaving = false;

    this.errorMessage = err.error?.detail || err.error?.message || err.error?.title || 'An error occurred while saving the program.';
    window.scrollTo({ top: 0, behavior: 'smooth' });
    console.error(err);

  }
}