import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

import { ProgramManagerService } from '../../services/program-manager.service';
import { ResourceUtilisation } from '../../models/program.model';
import { ResourceNavbarComponent } from '../resource-navbar.component/resource-navbar.component';

@Component({
  selector: 'app-utilisation-report',
  standalone: true,
  imports: [CommonModule, RouterModule, ResourceNavbarComponent],
  templateUrl: './utilisation-report.component.html',
  styleUrls: ['./utilisation-report.component.css']
})
export class UtilisationReportComponent implements OnInit {
  private programService = inject(ProgramManagerService);

  reportData: ResourceUtilisation[] = [];
  isLoading = true;
  today: Date = new Date();

  ngOnInit(): void {
    this.loadReport();
  }

  loadReport() {
    this.isLoading = true;
    this.programService.getUtilisationReport().subscribe({
      next: (data: any) => {
        // Handle potential C# wrappers (like { data: [...] } or returning the array directly)
        this.reportData = data.data || data || [];
        this.isLoading = false;
      },
      error: (err) => {
        console.error("Failed to load utilisation report", err);
        this.isLoading = false;
      }
    });
  }

  // Triggers the native browser print dialog!
  printReport() {
    window.print();
  }
}