import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common'; 
import { RouterModule } from '@angular/router'; 
import { WelfareOfficerService } from '../../services/welfare-officer services';
import { WelfareApplication, DashboardStats } from '../../models/welfare-officer models';
import { DeleteConfirmComponent } from '../delete-confirm/delete-confirm.component';
import{WelfareApplicationNavbarComponent} from '../welfare-application-navbar.component/welfare-application-navbar.component';
@Component({
  selector: 'app-dashboard',
  standalone: true, 
  imports: [CommonModule, RouterModule, DeleteConfirmComponent, WelfareApplicationNavbarComponent ], 
  templateUrl: './dashboard.component.html', 
  styleUrls: ['./dashboard.component.css'] 
})
export class DashboardComponent implements OnInit {
  applications: WelfareApplication[] = []; 
  // 1. Replaced any[] with WelfareApplication[]
  filteredList: WelfareApplication[] = [];
  
  currentView: string = 'All'; // Tracks if we are in 'All' or 'Pending' view
  stats: DashboardStats = { total: 0, pending: 0, approved: 0, rejected: 0 };

  showDeleteModal = false;
  // 2. Replaced any with WelfareApplication | null (since it starts as null)
  selectedAppForDelete: WelfareApplication | null = null;

  constructor(private welfareService: WelfareOfficerService) {}

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.welfareService.getApplications().subscribe({
      // 3. Explicitly typed the incoming data as WelfareApplication[]
      next: (data: WelfareApplication[]) => {
        this.applications = Array.isArray(data) ? data : [];
        this.calculateStats();
        this.setView('All'); // Default view
        console.log(data);
      },
      error: (err) => console.error('Connection Error:', err)
    });
  }

  calculateStats(): void {
    this.stats.total = this.applications.length;
    this.stats.pending = this.applications.filter(a => a.status === 'Pending').length;
    this.stats.approved = this.applications.filter(a => a.status === 'Approved' || a.status === 'Fully Disbursed').length;
    this.stats.rejected = this.applications.filter(a => a.status === 'Rejected').length;
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
  }

  // Delete Modal Logic
  // 4. Replaced app: any with app: WelfareApplication
  openDeleteConfirm(app: WelfareApplication) {
    this.selectedAppForDelete = app;
    this.showDeleteModal = true;
  }

  closeModal() {
    this.showDeleteModal = false;
    this.selectedAppForDelete = null;
  }

  handleDelete(id: number) {
    this.welfareService.deleteApplication(id).subscribe({
      next: () => {
        this.applications = this.applications.filter(a => a.applicationID !== id);
        this.setView(this.currentView); // Refresh current view
        this.calculateStats();
        this.closeModal();
      },
      error: (err) => {
        console.error('Delete failed', err);
        this.closeModal();
      }
    });
  }
}