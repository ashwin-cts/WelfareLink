import { Component, OnInit, inject } from '@angular/core';

import { RouterModule, Router } from '@angular/router';

@Component({
  selector: 'app-auditor-navbar',
  standalone: true,
  imports: [RouterModule],
  templateUrl: './auditor-navbar.component.html',
  styleUrls: ['./auditor-navbar.component.css'],
})
export class AuditorNavbarComponent implements OnInit {
  public router = inject(Router);
  userName: string = 'Auditor';

  // --- Dropdown State ---
  isDropdownOpen = false;

  toggleDropdown() {
    this.isDropdownOpen = !this.isDropdownOpen;
  }

  closeDropdown() {
    this.isDropdownOpen = false;
  }
  // ----------------------

  ngOnInit(): void {
    const storedName = localStorage.getItem('userName');
    if (storedName) {
      this.userName = storedName;
    }
  }

  goToDashboard(event: Event) {
    event.preventDefault();

    if (this.router.url === '/auditor-dashboard') {
      // Force reload to refresh dashboard data if already there
      this.router.navigateByUrl('/', { skipLocationChange: true }).then(() => {
        this.router.navigate(['/auditor-dashboard']);
      });
    } else {
      this.router.navigate(['/auditor-dashboard']);
    }
  }

  logout(): void {
    localStorage.clear();
    this.router.navigate(['/login']);
  }
}
