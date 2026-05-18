import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';

@Component({
  selector: 'app-compliance-officer-profile',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './compliance-officer-profile.component.html',
  styleUrls: ['./compliance-officer-profile.component.css']
})
export class ComplianceOfficerProfileComponent {
  isAccountDropdownOpen = false;

  constructor(private router: Router) { }

  toggleDropdown(event: Event) {
    event.preventDefault();
    this.isAccountDropdownOpen = !this.isAccountDropdownOpen;
  }

  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('jwt');
    localStorage.removeItem('userRole');
    localStorage.removeItem('currentUser');
    localStorage.clear();
    this.router.navigate(['/login']);
  }
}