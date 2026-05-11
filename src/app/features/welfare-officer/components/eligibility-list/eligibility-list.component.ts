import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms'; // 1. Added FormsModule for the filter dropdown
import { WelfareOfficerService } from '../../services/welfare-officer services';
import { WelfareApplicationNavbarComponent } from '../welfare-application-navbar.component/welfare-application-navbar.component';
import { EligibilityCheck } from '../../models/welfare-officer models'; // 2. Imported your interface

@Component({
  selector: 'app-eligibility-list',
  standalone: true,
  // 3. Added FormsModule to imports
  imports: [CommonModule, RouterModule, FormsModule, WelfareApplicationNavbarComponent],
  templateUrl: './eligibility-list.component.html',
  styleUrls: ['./eligibility-list.component.css']
})
export class EligibilityListComponent implements OnInit {
  // 4. Replaced 'any[]' with strictly typed interface arrays
  checks: EligibilityCheck[] = []; 
  filteredChecks: EligibilityCheck[] = []; 
  
  // 5. Property to track the selected dropdown value
  selectedResult: string = 'All Results'; 

  constructor(
    private welfareService: WelfareOfficerService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadChecks();
  }

  loadChecks(): void {
    // 6. Strongly typed the incoming data
    this.welfareService.getAllEligibilityChecks().subscribe({
      next: (data: EligibilityCheck[]) => {
        this.checks = data;
        this.filteredChecks = [...this.checks]; // Initialize the table with all data
        console.log('Retrieved Checks:', this.checks);
      },
      error: (err) => console.error('Error fetching checks:', err)
    });
  }

  // 7. Added the filter logic
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

  // 8. Replaced 'any' with 'number' for IDs
  handleEdit(applicationId: number): void {
    if (applicationId) {
      this.router.navigate(['/eligibility-edit', applicationId]);
    } else {
      console.error('Cannot edit: Application ID is missing.');
      alert('Error: This record does not have a valid Application ID.');
    }
  }

  handleDelete(id: number): void {
    if(confirm('Are you sure you want to delete this check?')) {
      // Logic for deletion service call would go here
    }
  }
}