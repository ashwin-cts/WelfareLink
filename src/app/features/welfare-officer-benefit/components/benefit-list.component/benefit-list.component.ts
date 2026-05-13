import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { BenefitService } from '../../services/benefit.service';
import { Benefit } from '../../models/benefit.model';
import { BenefitNavbarComponent } from '../benefit-navbar.component/benefit-navbar.component';

@Component({
  selector: 'app-benefit-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, BenefitNavbarComponent],
  templateUrl: './benefit-list.component.html',
  styleUrls: ['./benefit-list.component.css']
})
export class BenefitListComponent implements OnInit {
  private benefitService = inject(BenefitService);

  benefits: Benefit[] = [];
  filteredBenefits: Benefit[] = [];

  isLoading = true;
  searchTerm = '';

  ngOnInit(): void {
    this.loadBenefits();
  }

  loadBenefits(): void {
    this.isLoading = true;
    this.benefitService.getAllBenefits().subscribe({
      next: (data) => {
        this.benefits = data;
        this.filteredBenefits = data;
        this.isLoading = false;

        // NEW: Fetch the Application & Citizen details for EACH benefit in the list
        this.benefits.forEach(benefit => {
          if (benefit.applicationID) {
            this.benefitService.getApplicationById(benefit.applicationID).subscribe({
              next: (appData) => {
                benefit.welfareApplication = appData;
              },
              error: (err) => console.error('Error fetching app for benefit', benefit.benefitID, err)
            });
          }
        });
      },
      error: (err) => {
        console.error('Error fetching benefits:', err);
        this.isLoading = false;
      }
    });
  }

  // Live search functionality
  filterBenefits(): void {
    if (!this.searchTerm) {
      this.filteredBenefits = this.benefits;
    } else {
      const term = this.searchTerm.toLowerCase();
      this.filteredBenefits = this.benefits.filter(b =>
        b.benefitID.toString().includes(term) ||
        (b.type && b.type.toLowerCase().includes(term)) ||
        (b.status && b.status.toLowerCase().includes(term))
      );
    }
  }

  // Match C# status to CSS Class
  getStatusClass(status?: string): string {
    const s = (status || '').toLowerCase();
    if (s.includes('allocated')) return 'status-allocated';
    if (s.includes('fully disbursed') || s === 'disbursed') return 'status-disbursed';
    if (s.includes('partially disbursed')) return 'status-pending';
    if (s.includes('failed')) return 'status-failed';
    return 'status-pending';
  }

  // Deletes the benefit and updates UI instantly
  onDelete(id: number): void {
    if (confirm('Are you sure you want to delete Benefit #' + id + '?')) {
      this.benefitService.deleteBenefit(id).subscribe({
        next: () => {
          this.benefits = this.benefits.filter(b => b.benefitID !== id);
          this.filterBenefits();
        },
        error: (err) => {
          console.error('Error deleting benefit:', err);
          alert('Failed to delete the benefit. Please try again.');
        }
      });
    }
  }
}