import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { WelfareOfficerService } from '../../services/welfare-officer services';
import { WelfareApplicationNavbarComponent } from '../welfare-application-navbar.component/welfare-application-navbar.component';
import { EligibilityCheck } from '../../models/welfare-officer models';

// 1. IMPORT YOUR CUSTOM MODAL
import { DeleteConfirmComponent } from '../delete-confirm/delete-confirm.component';

@Component({
  selector: 'app-eligibility-list',
  standalone: true,
  // 2. ADD TO IMPORTS ARRAY
  imports: [CommonModule, RouterModule, FormsModule, WelfareApplicationNavbarComponent, DeleteConfirmComponent],
  templateUrl: './eligibility-list.component.html',
  styleUrls: ['./eligibility-list.component.css']
})
export class EligibilityListComponent implements OnInit {
  checks: EligibilityCheck[] = []; 
  filteredChecks: EligibilityCheck[] = []; 
  selectedResult: string = 'All Results'; 

  // 3. MODAL STATE VARIABLES
  showDeleteModal = false;
  selectedCheckForDelete: any = null;

  constructor(
    private welfareService: WelfareOfficerService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadChecks();
  }

  loadChecks(): void {
    this.welfareService.getAllEligibilityChecks().subscribe({
      next: (data: EligibilityCheck[]) => {
        this.checks = data;
        this.filteredChecks = [...this.checks]; 
      },
      error: (err) => console.error('Error fetching checks:', err)
    });
  }

  applyFilter(): void {
    if (this.selectedResult === 'All Results') {
      this.filteredChecks = [...this.checks];
    } else {
      this.filteredChecks = this.checks.filter(check => 
        check.result?.toLowerCase() === this.selectedResult.toLowerCase()
      );
    }
  }

  goBack(): void {
    this.router.navigate(['/welfare-officer/dashboard']);
  }

  handleEdit(applicationId: number): void {
    if (applicationId) {
      this.router.navigate(['/eligibility-edit', applicationId]);
    }
  }

  // --- 4. NEW MODAL LOGIC ---

  openDeleteConfirm(check: EligibilityCheck): void {
    // We map the checkID to 'applicationID' so your existing modal reads it correctly without throwing an error
    this.selectedCheckForDelete = {
      applicationID: check.checkID, 
      citizen: { firstName: 'Eligibility', lastName: 'Assessment' } // Placeholder name for the modal UI
    };
    this.showDeleteModal = true;
  }

  closeModal(): void {
    this.showDeleteModal = false;
    this.selectedCheckForDelete = null;
  }

  handleDelete(id: number): void {
    this.welfareService.deleteEligibilityCheck(id).subscribe({
      next: () => {
        // Remove from both arrays to update UI instantly
        this.checks = this.checks.filter(c => c.checkID !== id);
        this.applyFilter(); 
        
        this.closeModal(); // Hide the modal
      },
      error: (err) => {
        console.error('Failed to delete check:', err);
        alert('Could not delete the check. Please try again.');
        this.closeModal();
      }
    });
  }
}