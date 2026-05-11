import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms'; 
import { WelfareOfficerService } from '../../services/welfare-officer services';
import { WelfareApplication } from '../../models/welfare-officer models';

@Component({
  selector: 'app-application-form',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './application-form.component.html',
  styleUrls: ['./application-form.component.css']
})
export class ApplicationFormComponent implements OnInit {
  // Use any[] to handle nested Citizen/Program objects from your API
  applications: any[] = [];
  filteredList: any[] = [];
  selectedStatus: string = 'All Status';
  loading: boolean = true;

  constructor(private welfareService: WelfareOfficerService) {}

  ngOnInit(): void {
    this.loadApplications();
  }

  /**
   * Fetches the initial data from the C# Backend
   */
  loadApplications(): void {
    this.welfareService.getApplications().subscribe({
      next: (data) => {
        console.log('Data received from API:', data);
        this.applications = Array.isArray(data) ? data : [];
        this.filteredList = [...this.applications];
        this.loading = false;
      },
      error: (err) => {
        console.error('Connection Error:', err);
        this.loading = false;
      }
    });
  }

  /**
   * Filters the list based on the dropdown selection.
   * Triggered by the (change) event in the HTML.
   */
  applyFilter(): void {
    console.log('Filtering for:', this.selectedStatus); // Debug to see what's selected

    if (this.selectedStatus === 'All Status') {
        this.filteredList = [...this.applications];
    } else {
        // Ensure comparison matches the exact string from your API
        // We use .trim() to handle any accidental whitespace from the database
        this.filteredList = this.applications.filter(app => 
            app.status?.trim() === this.selectedStatus.trim()
        );
    }
  }
}