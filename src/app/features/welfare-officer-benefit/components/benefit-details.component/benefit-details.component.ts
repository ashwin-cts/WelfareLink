import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { BenefitService } from '../../services/benefit.service';
import { Benefit } from '../../models/benefit.model';
import { BenefitNavbarComponent } from '../benefit-navbar.component/benefit-navbar.component';
import { BenefitFormComponent } from "../benefit-form.component/benefit-form.component";

// Benefit Details Component
@Component({
  selector: 'app-benefit-details',
  standalone: true,
  imports: [CommonModule, RouterModule, BenefitNavbarComponent],
  templateUrl: './benefit-details.component.html',
  styleUrls: ['./benefit-details.component.css'] // Assuming you have standard styles here
})
export class BenefitDetailsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private benefitService = inject(BenefitService);

  benefit: Benefit | null = null;
  isLoading = true;

  ngOnInit(): void {
    // Grab the ID from the URL route (e.g., /benefit/details/5)
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) {
      this.loadBenefitDetails(id);
    } else {
      // If no ID is found, send them back to the list
      this.router.navigate(['../'], { relativeTo: this.route });
    }
  }

  loadBenefitDetails(id: number): void {
    this.isLoading = true;
    this.benefitService.getBenefitById(id).subscribe({
      next: (data) => {
        this.benefit = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error fetching benefit details', err);
        this.isLoading = false;
      }
    });
  }

  // Translating the C# switch statement for status colors
  getStatusClass(status?: string): string {
    const s = (status || '').toLowerCase();
    if (s.includes('allocated')) return 'status-allocated';
    if (s.includes('fully disbursed') || s === 'disbursed') return 'status-disbursed';
    if (s.includes('failed')) return 'status-failed';
    return 'status-pending';
  }

  // Replacing the Delete.cshtml page with a clean popup function
  onDelete(): void {
    if (!this.benefit) return;

    const confirmDelete = window.confirm('Are you sure you want to permanently delete this benefit? This action cannot be undone.');

    if (confirmDelete) {
      this.benefitService.deleteBenefit(this.benefit.benefitID).subscribe({
        next: () => {
          // On success, navigate back to the list
          this.router.navigate(['../../'], { relativeTo: this.route });
        },
        error: (err) => {
          console.error('Error deleting benefit', err);
          alert('Failed to delete the benefit. Please try again.');
        }
      });
    }
  }
}