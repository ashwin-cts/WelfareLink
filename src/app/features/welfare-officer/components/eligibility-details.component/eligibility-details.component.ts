import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { WelfareOfficerService } from '../../services/welfare-officer services';
import { WelfareApplicationNavbarComponent } from '../welfare-application-navbar.component/welfare-application-navbar.component';
@Component({
  selector: 'app-eligibility-details',
  standalone: true,
  imports: [CommonModule, RouterModule, WelfareApplicationNavbarComponent],
  templateUrl: './eligibility-details.component.html',
  styleUrls: ['./eligibility-details.component.css']
})
export class EligibilityDetailsComponent implements OnInit {
  check: any;
  loading: boolean = true;

  constructor(
    private route: ActivatedRoute,
    private welfareService: WelfareOfficerService
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.loadDetails(id);
  }

  loadDetails(id: number): void {
    this.welfareService.getEligibilityCheckById(id).subscribe({
      next: (data: any) => {
        this.check = data;
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading eligibility details:', err);
        this.loading = false;
      }
    });
  }
}