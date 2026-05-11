import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms'; 
import { WelfareOfficerService } from '../../services/welfare-officer services';
import { WelfareApplication, EligibilityCheck } from '../../models/welfare-officer models';
import{WelfareApplicationNavbarComponent} from '../welfare-application-navbar.component/welfare-application-navbar.component';
@Component({
  selector: 'app-details',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, WelfareApplicationNavbarComponent],
  templateUrl: './details.component.html',
  styleUrls: ['./details.component.css']
})
export class DetailsComponent implements OnInit {
  application: WelfareApplication | null = null;
  eligibilityHistory: EligibilityCheck[] = [];
  loading: boolean = true;
  
  // Property bound to [(ngModel)] in the Update Status Modal
  newStatus: string = ''; 

  constructor(
    private route: ActivatedRoute,
    private welfareService: WelfareOfficerService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadDetails(+id);
    }
  }

  /**
   * Loads Application details and Eligibility history sequentially
   */
  loadDetails(id: number): void {
    this.loading = true;
    this.welfareService.getApplicationById(id).subscribe({
      next: (data: WelfareApplication) => {
        this.application = data;
        
        // Populate history from nested data OR separate API call
        if (data.eligibilityChecks) {
          this.eligibilityHistory = data.eligibilityChecks;
        } else {
          this.fetchHistoryFromApi(id);
        }
        
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading application details:', err);
        this.loading = false;
      }
    });
  }

  /** Optional: Fetch history if not included in the main application object */
  private fetchHistoryFromApi(id: number): void {
    this.welfareService.getEligibilityHistory(id).subscribe({
      next: (history) => this.eligibilityHistory = history,
      error: (err) => console.error('Error loading history:', err)
    });
  }

  /**
   * Called by the "Update Status" button inside the Modal
   */
  onUpdateStatus(): void {
    if (!this.application || !this.newStatus) {
      return;
    }

    this.welfareService.updateApplicationStatus(this.application.applicationID, this.newStatus).subscribe({
      next: () => {
        // Success: Re-fetch details to update UI badges and timeline
        if (this.application) {
          const currentId = this.application.applicationID;
          this.loadDetails(currentId);
          this.newStatus = ''; // Reset modal dropdown for next use
        }
      },
      error: (err) => {
        console.error('Failed to update status', err);
        alert('Could not update status. Please check your connection.');
      }
    });
  }

  /**
   * Helper to return CSS class names based on status string
   * Used for [ngClass] in the HTML template
   */
  getStatusClass(status: string | undefined): string {
    if (!status) return 'status-pending';
    // Converts "Under Review" to "status-under-review"
    const formattedStatus = status.toLowerCase().replace(/\s+/g, '-');
    return `status-${formattedStatus}`;
  }
}