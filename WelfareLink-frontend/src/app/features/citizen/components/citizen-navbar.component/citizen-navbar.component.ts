import { Component, OnInit, inject } from '@angular/core';

import { RouterModule, Router } from '@angular/router';

@Component({
  selector: 'app-citizen-navbar',
  standalone: true,
  imports: [RouterModule],
  templateUrl: './citizen-navbar.component.html',
})
export class CitizenNavbarComponent implements OnInit {
  public router = inject(Router);
  userName: string = 'Citizen';

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

    if (this.router.url === '/citizen-dashboard') {
      // Force reload to refresh dashboard data if already there
      this.router.navigateByUrl('/', { skipLocationChange: true }).then(() => {
        this.router.navigate(['/citizen-dashboard']);
      });
    } else {
      this.router.navigate(['/citizen-dashboard']);
    }
  }

  logout(): void {
    localStorage.clear();
    this.router.navigate(['/login']);
  }
}
