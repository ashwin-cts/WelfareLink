import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
// 1. IMPORT Router
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { WelfareOfficerService } from '../../services/welfare-officer.services';
import { WelfareApplicationNavbarComponent } from '../welfare-application-navbar.component/welfare-application-navbar.component';
import { DeleteConfirmComponent } from '../delete-confirm/delete-confirm.component';

@Component({
  selector: 'app-eligibility-details',
  standalone: true,
  imports: [CommonModule, RouterModule, WelfareApplicationNavbarComponent, DeleteConfirmComponent],
  templateUrl: './eligibility-details.component.html',
  styleUrls: ['./eligibility-details.component.css']
})
export class EligibilityDetailsComponent implements OnInit {
  check: any;
  loading: boolean = true;

  // 2. MODAL STATE VARIABLES
  showDeleteModal = false;
  selectedCheckForDelete: any = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router, // 3. INJECT ROUTER
    private welfareService: WelfareOfficerService
  ) { }

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.loadDetails(id);
  }

  loadDetails(id: number): void {
    this.welfareService.getEligibilityCheckById(id).subscribe({
      next: (data: any) => {
        this.check = data;
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading eligibility details:', err);
        this.loading = false;
      }
    });
  }

  // --- 4. NEW DELETE MODAL LOGIC ---

  openDeleteConfirm(): void {
    if (!this.check) return;

    // Map the data so the existing modal reads it correctly
    this.selectedCheckForDelete = {
      applicationID: this.check.checkID,
      citizen: { firstName: 'Eligibility', lastName: 'Assessment' }
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
        this.closeModal();

        // 5. Navigate back to the list because this record is gone!
        this.router.navigate(['/eligibility-list']);
      },
      error: (err) => {
        console.error('Failed to delete check:', err);
        alert('Could not delete the check. Please try again.');
        this.closeModal();
      }
    });
  }
}