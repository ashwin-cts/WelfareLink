import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-program-manager-navbar',
  standalone: true,
  imports: [RouterModule],
  templateUrl: './program-manager-navbar.component.html',
  styleUrls: ['./program-manager-navbar.component.css'],
})
export class ProgramManagerNavbarComponent implements OnInit {
  public router = inject(Router);

  // Signals to hold API-driven user data
  userName = signal<string>('Loading...');
  userRole = signal<string>('Guest');

  // --- NEW: Dropdown State ---
  isDropdownOpen = false;

  toggleDropdown() {
    this.isDropdownOpen = !this.isDropdownOpen;
  }

  closeDropdown() {
    this.isDropdownOpen = false;
  }
  // ---------------------------

  ngOnInit(): void {
    const savedName = localStorage.getItem('userName');
    const savedRole = localStorage.getItem('userRole');

    if (savedName) this.userName.set(savedName);
    if (savedRole) this.userRole.set(savedRole);
  }

  goToDashboard(event: Event) {
    event.preventDefault();
    if (this.router.url === '/program-manager/dashboard') {
      this.router.navigateByUrl('/', { skipLocationChange: true }).then(() => {
        this.router.navigate(['/program-manager/dashboard']);
      });
    } else {
      this.router.navigate(['/program-manager/dashboard']);
    }
  }

  logout() {
    localStorage.clear();
    this.router.navigate(['/login']);
  }
}
