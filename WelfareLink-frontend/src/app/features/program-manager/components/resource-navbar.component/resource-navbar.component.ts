import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-resource-navbar',
  standalone: true,
  imports: [RouterModule],
  templateUrl: './resource-navbar.component.html',
  styleUrls: ['./resource-navbar.component.css'],
})
export class ResourceNavbarComponent implements OnInit {
  public router = inject(Router);

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

  ngOnInit(): void {
    const savedName = localStorage.getItem('userName');
    const savedRole = localStorage.getItem('userRole');

    if (savedName) this.userName.set(savedName);
    if (savedRole) this.userRole.set(savedRole);
  }

  logout() {
    localStorage.clear();
    this.router.navigate(['/login']);
  }
}
