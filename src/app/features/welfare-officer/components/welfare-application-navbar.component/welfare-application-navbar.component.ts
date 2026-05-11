import { Component, OnInit, inject, signal, HostListener, ElementRef } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-welfare-application-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './welfare-application-navbar.component.html',
  styleUrls: ['./welfare-application-navbar.component.css']
})
export class WelfareApplicationNavbarComponent implements OnInit {
  public router = inject(Router);
  
  userName = signal<string>('Loading...');
  userRole = signal<string>('Welfare Officer');

  // 1. ADD THIS: Variable to track if dropdown is open
  isDropdownOpen = false;

  // 2. ADD THIS: Inject ElementRef so Angular knows where this component is on the screen
  constructor(private eRef: ElementRef) {}

  ngOnInit(): void {
    const savedName = localStorage.getItem('userName');
    const savedRole = localStorage.getItem('userRole');

    if (savedName) this.userName.set(savedName);
    else this.userName.set('Officer John Doe'); 

    if (savedRole) this.userRole.set(savedRole);
  }

  // 3. ADD THIS: Method to manually toggle the dropdown
  toggleDropdown(event: Event) {
    event.preventDefault(); // Prevents the page from jumping to top
    this.isDropdownOpen = !this.isDropdownOpen;
  }

  // 4. ADD THIS: Closes the dropdown if you click anywhere else on the page
  @HostListener('document:click', ['$event'])
  clickout(event: Event) {
    if (!this.eRef.nativeElement.contains(event.target)) {
      this.isDropdownOpen = false;
    }
  }

  goToDashboard(event: Event) {
    event.preventDefault();
    if (this.router.url === '/welfare-officer/dashboard') {
      this.router.navigateByUrl('/', { skipLocationChange: true }).then(() => {
          this.router.navigate(['/welfare-officer/dashboard']);
      });
    } else {
      this.router.navigate(['/welfare-officer/dashboard']);
    }
  }

  logout() {
    localStorage.clear();
    this.router.navigate(['/login']);
  }
}