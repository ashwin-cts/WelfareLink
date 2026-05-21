import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { WelfareOfficerService } from '../../services/welfare-officer.services';
import { WelfareApplication, DashboardStats, ComplianceRecord } from '../../models/welfare-officer.models';
import { WelfareApplicationNavbarComponent } from '../welfare-application-navbar.component/welfare-application-navbar.component';
@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, WelfareApplicationNavbarComponent],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  applications: WelfareApplication[] = [];
  complianceRecords: ComplianceRecord[] = [];
  filteredList: WelfareApplication[] = [];

  isSortDescending: boolean = true; // Default to newest first
  currentView: string = 'All'; // Tracks if we are in 'All' or 'Pending' view
  stats: DashboardStats = { total: 0, pending: 0, approved: 0, rejected: 0, fullyDisbursed: 0 };
  isLoading: boolean = true; // Set to true initiallyisLoading: boolean = true; // Set to true initially
  showDeleteModal = false;
  // 2. Replaced any with WelfareApplication | null (since it starts as null)
  selectedAppForDelete: WelfareApplication | null = null;

  showComplianceModal = false;
  selectedAppCompliance: any[] = [];
  selectedCitizenNameForCompliance: string = '';
  selectedAppIdForCompliance: number | null = null;
  constructor(private welfareService: WelfareOfficerService) { }

  ngOnInit(): void {
    this.loadData();
    this.loadComplianceData();
  }

  loadData(): void {
    this.isLoading = true;
    this.welfareService.getApplications().subscribe({
      // 3. Explicitly typed the incoming data as WelfareApplication[]
      next: (data: WelfareApplication[]) => {
        this.isLoading = false;
        this.applications = Array.isArray(data) ? data : [];
        this.calculateStats();
        this.setView('All'); // Default view
        console.log(data);
      },
      error: (err) => {
        this.isLoading = false;
        console.error('Connection Error:', err);
      }
    });
  }
  loadComplianceData(): void {
    this.welfareService.getComplianceRecords().subscribe({
      next: (records: ComplianceRecord[]) => {
        console.log(records);
        this.complianceRecords = records;
      },
      error: (err) => console.error('Failed to load compliance records', err)
    });
  }
  hasOpenCompliance(appId: number): boolean {
    return this.complianceRecords.some((r: any) =>
      r.applicationID === appId && r.status === 'Open'
    );
  }
  openComplianceModal(app: WelfareApplication) {
    this.selectedCitizenNameForCompliance = app.citizen?.name || 'Unknown Citizen';

    // Filter records using applicationID from the console log structure
    this.selectedAppCompliance = this.complianceRecords.filter((r: any) =>
      r.applicationID === app.applicationID
    );
    this.showComplianceModal = true;
  }

  closeComplianceModal() {
    this.showComplianceModal = false;
    this.selectedAppCompliance = [];
    this.selectedCitizenNameForCompliance = ''; // Reset name
  }
  toggleDateSort() {
    this.isSortDescending = !this.isSortDescending;
    this.applySorting();
  }

  // Extract the sorting logic into its own method so it can be reused

  calculateStats(): void {
    this.stats.total = this.applications.length;
    this.stats.pending = this.applications.filter(a => a.status === 'Pending').length;
    this.stats.approved = this.applications.filter(a => a.status === 'Approved').length;
    this.stats.rejected = this.applications.filter(a => a.status === 'Rejected').length;
    this.stats.fullyDisbursed = this.applications.filter(a => a.status === 'Fully Disbursed').length;

  }

  /**
   * Sets the view and filters the list accordingly
   * @param mode 'All', 'Pending', 'Approved', 'Rejected'
   */
  setView(mode: string): void {
    this.currentView = mode;
    if (mode === 'All') {
      this.filteredList = [...this.applications];
    } else {
      this.filteredList = this.applications.filter(a => a.status === mode);
    }
    this.applySorting();
  }
  applySorting() {
    this.filteredList.sort((a, b) => {
      const dateA = new Date(a.submittedDate).getTime();
      const dateB = new Date(b.submittedDate).getTime();

      // If descending, B - A (Newest first). If ascending, A - B (Oldest first)
      return this.isSortDescending ? (dateB - dateA) : (dateA - dateB);
    });
  }


  // // Delete Modal Logic
  // openDeleteConfirm(app: WelfareApplication) {
  //   this.selectedAppForDelete = app;
  //   this.showDeleteModal = true;
  // }

  // closeModal() {
  //   this.showDeleteModal = false;
  //   this.selectedAppForDelete = null;
  // }

  // handleDelete(id: number) {
  //   this.welfareService.deleteApplication(id).subscribe({
  //     next: () => {
  //       this.applications = this.applications.filter(a => a.applicationID !== id);
  //       this.setView(this.currentView); // Refresh current view
  //       this.calculateStats();
  //       this.closeModal();
  //     },
  //     error: (err) => {
  //       console.error('Delete failed', err);
  //       this.closeModal();
  //     }
  //   });
  // }
}