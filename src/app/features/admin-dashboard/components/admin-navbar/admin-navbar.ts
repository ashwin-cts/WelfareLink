import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-admin-navbar',
  standalone: true,
  imports: [RouterModule],
  templateUrl: './admin-navbar.html',
  styleUrls: ['./admin-navbar.css'],
})
export class AdminNavbarComponent implements OnInit {
  public router = inject(Router);

  userName = signal<string>('Admin User');
  userRole = signal<string>('Admin');

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
    const savedName = localStorage.getItem('userName');
    const savedRole = localStorage.getItem('userRole');

    if (savedName) this.userName.set(savedName);
    if (savedRole) this.userRole.set(savedRole);
  }

  goToDashboard(event: Event) {
    event.preventDefault();

    if (this.router.url === '/admin-dashboard') {
      this.router.navigateByUrl('/', { skipLocationChange: true }).then(() => {
        this.router.navigate(['/admin-dashboard']);
      });
    } else {
      this.router.navigate(['/admin-dashboard']);
    }
  }

  logout() {
    localStorage.clear();
    this.router.navigate(['/login']);
  }
}
