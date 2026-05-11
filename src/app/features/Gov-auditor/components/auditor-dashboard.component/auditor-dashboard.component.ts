import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuditorService, AuditorDashboardStats, BudgetMonitoringItem, ResourceStatementItem, DisbursementStatementItem } from '../services/auditor.service';
import { AuditorNavbarComponent } from '../auditor-navbar.component/auditor-navbar.component';
//import { Component, OnInit } from '@angular/core';
//import { CommonModule } from '@angular/common';
//import { AuditorService, AuditorDashboardStats, BudgetMonitoringItem, ResourceStatementItem, DisbursementStatementItem } from '../../services/auditor.service';
//import { AuditorNavbarComponent } from '../auditor-navbar/auditor-navbar.component';
// IMPORT THE NEW CHILDREN
import { SummaryCardsComponent } from '../summary-cards.component/summary-cards.component';
import { BudgetTableComponent } from '../budget-table.component/budget-table.component';
import { ResourceHistoryComponent } from '../resource-history.component/resource-history.component';
import { DisbursementHistoryComponent } from '../disbursement-history.component/disbursement-history.component';

@Component({
  selector: 'app-auditor-dashboard',
  standalone: true,
  imports: [
    CommonModule, 
    AuditorNavbarComponent,
    SummaryCardsComponent,
    BudgetTableComponent,
    ResourceHistoryComponent,
    DisbursementHistoryComponent
  ],
  templateUrl: './auditor-dashboard.component.html',
  styleUrls: ['./auditor-dashboard.component.css']
})
export class AuditorDashboardComponent implements OnInit {
  activeTab: 'dashboard' | 'budget' | 'resource' | 'disbursement' = 'dashboard';
  errorMessage: string | null = null;

  // Data for the different tabs
  dashboardStats: AuditorDashboardStats | null = null;
  budgetItems: BudgetMonitoringItem[] = [];
  resourceItems: ResourceStatementItem[] = [];
  disbursementItems: DisbursementStatementItem[] = []; // You'll need to define this data structure

  constructor(private auditorService: AuditorService) {}

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
      // 1. Explicitly type 'data' as AuditorDashboardStats
      next: (data: AuditorDashboardStats) => this.dashboardStats = data,
      // 2. Explicitly type 'err' as unknown or any (unknown is safer)
      error: (err: unknown) => {
        console.error(err); // Good practice to log it
        this.errorMessage = 'Failed to load dashboard statistics.';
      }
    });
  }

  loadBudgetData() {
    this.auditorService.getBudgetMonitoring().subscribe({
      // Type as an array of BudgetMonitoringItem
      next: (data: BudgetMonitoringItem[]) => this.budgetItems = data,
      error: (err: unknown) => {
        console.error(err);
        this.errorMessage = 'Failed to load budget monitoring data.';
      }
    });
  }

  loadResourceData() {
    this.auditorService.getResourceStatement().subscribe({
      // Type as an array of ResourceStatementItem
      next: (data: ResourceStatementItem[]) => this.resourceItems = data,
      error: (err: unknown) => {
        console.error(err);
        this.errorMessage = 'Failed to load resource statement data.';
      }
    });
  }

  loadDisbursementData() {
     this.auditorService.getDisbursementStatement().subscribe({
      // Type as an array of DisbursementStatementItem
      next: (data: DisbursementStatementItem[]) => this.disbursementItems = data,
      error: (err: unknown) => {
        console.error(err);
        this.errorMessage = 'Failed to load disbursement statement data.';
      }
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
      const values = headers.map(header => {
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