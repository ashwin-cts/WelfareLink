import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { WelfareOfficerService } from '../../services/welfare-officer services';
import { EligibilityCheck } from '../../models/welfare-officer models';
import { WelfareApplicationNavbarComponent } from '../welfare-application-navbar.component/welfare-application-navbar.component';
@Component({
  selector: 'app-eligibility-edit',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, WelfareApplicationNavbarComponent],
  templateUrl: './eligibility-edit.component.html',
  styleUrls: ['./eligibility-edit.component.css']
})
export class EligibilityEditComponent implements OnInit {
  // Property names match the EligibilityCheck interface exactly
  check: EligibilityCheck = {
    checkID: 0,
    applicationId: 0, 
    officerID: 0,
    result: '',
    resultCode: '',
    date: '',         
    notes: ''
  } as EligibilityCheck;
  
  loading = true;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private welfareService: WelfareOfficerService
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));

    if (id) {
      this.welfareService.getEligibilityCheckById(id).subscribe({
        next: (data) => {
          this.check = data;
          this.loading = false;
          console.log('Data loaded:', this.check);
        },
        error: (err) => {
          console.error('Error loading check:', err);
          this.loading = false;
        }
      });
    }
  }

  onSave(): void {
    if (this.check && this.check.checkID) {
      this.loading = true;
      this.welfareService.updateEligibilityCheck(this.check.checkID, this.check).subscribe({
        next: () => {
          this.router.navigate(['/eligibility-details', this.check.checkID]);
        },
        error: (err) => {
          console.error('Update failed:', err);
          this.loading = false;
          alert('Failed to save changes.');
        }
      });
    }
  }

  onCancel(): void {
    this.router.navigate(['/eligibility-list']);
  }
}