import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuditorDashboardStats, BudgetMonitoringItem, ResourceStatementItem, DisbursementStatementItem } from '../../models/auditor.model';
import { AuditorService } from '../services/auditor.service';

// IMPORT THE NEW COMPONENTS
import { AuditorNavbarComponent } from '../auditor-navbar.component/auditor-navbar.component';

// IMPORT THE NEW CHILDREN
import { SummaryCardsComponent } from '../summary-cards.component/summary-cards.component';
import { BudgetTableComponent } from '../budget-table.component/budget-table.component';
import { ResourceHistoryComponent } from '../resource-history.component/resource-history.component';
import { DisbursementHistoryComponent } from '../disbursement-history.component/disbursement-history.component';

@Component({
  selector: 'app-auditor-dashboard',
  standalone: true,
  imports: [
    AuditorNavbarComponent,
    SummaryCardsComponent,
    BudgetTableComponent,
    ResourceHistoryComponent,
    DisbursementHistoryComponent,
  ],
  templateUrl: './auditor-dashboard.component.html',
  styleUrls: ['./auditor-dashboard.component.css'],
})
export class AuditorDashboardComponent implements OnInit {
  activeTab: 'dashboard' | 'budget' | 'resource' | 'disbursement' = 'dashboard';
  errorMessage: string | null = null;

  // Data for the different tabs
  dashboardStats: AuditorDashboardStats | null = null;
  budgetItems: BudgetMonitoringItem[] = [];
  resourceItems: ResourceStatementItem[] = [];
  disbursementItems: DisbursementStatementItem[] = [];

  constructor(private auditorService: AuditorService) { }

  ngOnInit(): void {
    this.loadDashboardData();
  }

  setTab(tabName: 'dashboard' | 'budget' | 'resource' | 'disbursement') {
    this.activeTab = tabName;
    this.errorMessage = null; // Clear errors when switching tabs

    // Lazy load data based on the selected tab
    if (tabName === 'dashboard' && !this.dashboardStats) {
      this.loadDashboardData();
    } else if (tabName === 'budget' && this.budgetItems.length === 0) {
      this.loadBudgetData();
    } else if (tabName === 'resource' && this.resourceItems.length === 0) {
      this.loadResourceData();
    } else if (tabName === 'disbursement' && this.disbursementItems.length === 0) {
      this.loadDisbursementData();
    }
  }

  loadDashboardData() {
    this.auditorService.getDashboardStats().subscribe({
      // We use 'any' to read the PascalCase keys from C# and map them to camelCase
      next: (res: any) => {

        this.dashboardStats = {

          totalApplications: res.TotalApplications || res.totalApplications || 0,
          totalPrograms: res.TotalPrograms || res.totalPrograms || 0,
          totalBudget: res.TotalBudget || res.totalBudget || 0,
          totalResource: res.TotalResource || res.totalResource || 0,
          totalDisbursement: res.TotalDisbursement || res.totalDisbursement || 0,
        };
      },
      error: (err: unknown) => {
        console.error(err);
        this.errorMessage = 'Failed to load dashboard statistics.';
      },
    });
  }

  loadBudgetData() {
    this.auditorService.getBudgetMonitoring().subscribe({
      next: (res: any) => {
        // Map the array of C# objects to match your Angular interface
        this.budgetItems = res.map((item: any) => ({
          programID: item.ProgramID || item.programID,
          programName: item.ProgramName || item.programName,
          programStatus: item.ProgramStatus || item.programStatus,
          programBudget: item.ProgramBudget || item.programBudget || 0,
          allocatedResource: item.AllocatedResource || item.allocatedResource || 0,
          citizensApplied: item.CitizensApplied || item.citizensApplied || 0,
          totalDisbursed: item.TotalDisbursed || item.totalDisbursed || 0,
          remainingResource: item.RemainingResource || item.remainingResource || 0,
          utilizationPercent: item.UtilizationPercent || item.utilizationPercent || 0,
        }));
      },
      error: (err: unknown) => {
        console.error(err);
        this.errorMessage = 'Failed to load budget monitoring data.';
      },
    });
  }

  loadResourceData() {
    this.auditorService.getResourceStatement().subscribe({
      next: (res: any) => {
        this.resourceItems = res.map((item: any) => ({
          resourceID: item.ResourceID || item.resourceID,
          date: item.Date || item.date || new Date().toISOString(),
          programName: item.ProgramName || item.programName || 'Unknown Program',
          allocatedResource: item.AllocatedResource || item.allocatedResource || 0,
          remainingAllocationPending:
            item.RemainingAllocationPending || item.remainingAllocationPending || 0,
        }));
      },
      error: (err: unknown) => {
        console.error(err);
        this.errorMessage = 'Failed to load resource statement data.';
      },
    });
  }

  loadDisbursementData() {
    this.auditorService.getDisbursementStatement().subscribe({
      next: (res: any) => {
        console.log("Disbursement Data:", res); // debug

        // THE FIX: Your API already returns exactly what the HTML needs!
        // No .map() required. Just assign it directly.
        this.disbursementItems = res;

      },
      error: (err: unknown) => {
        console.error(err);
        this.errorMessage = 'Failed to load disbursement statement data.';
      },
    });
  }

  exportToCSV() {
    let dataToExport: any[] = [];
    let filename = 'export.csv';

    // Decide which data array to export based on the active tab
    if (this.activeTab === 'budget') {
      dataToExport = this.budgetItems;
      filename = 'budget-monitoring.csv';
    } else if (this.activeTab === 'resource') {
      dataToExport = this.resourceItems;
      filename = 'resource-statement.csv';
    } else if (this.activeTab === 'disbursement') {
      dataToExport = this.disbursementItems;
      filename = 'disbursement-statement.csv';
    } else {
      alert('Please navigate to Budget, Resource, or Disbursement tabs to export data.');
      return;
    }

    if (dataToExport.length === 0) {
      alert('No data available to export.');
      return;
    }

    // 1. Get dynamic headers from the object keys
    const headers = Object.keys(dataToExport[0]);

    // 2. Build the CSV string
    const csvRows = [];
    csvRows.push(headers.join(',')); // Add Header Row

    for (const row of dataToExport) {
      const values = headers.map((header) => {
        const val = row[header as keyof typeof row] || '';
        // Escape quotes to prevent breaking the CSV format
        return `"${String(val).replace(/"/g, '""')}"`;
      });
      csvRows.push(values.join(','));
    }

    const csvData = csvRows.join('\n');

    // 3. Trigger the browser download
    const blob = new Blob([csvData], { type: 'text/csv;charset=utf-8;' });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.setAttribute('href', url);
    link.setAttribute('download', filename);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }

  printReport() {
    window.print();
  }
}
