import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-citizen-navbar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './citizen-navbar.component.html',
  styleUrl: './citizen-navbar.component.css'
})
export class CitizenNavbarComponent {
  // Receives data FROM the dashboard
  @Input() activeTab: string = 'overview';
  @Input() citizenName: string = 'User';

  // Sends events TO the dashboard
  @Output() tabChange = new EventEmitter<string>();
  @Output() logoutTrigger = new EventEmitter<void>();

  isDropdownOpen = false;

  toggleDropdown() {
    this.isDropdownOpen = !this.isDropdownOpen;
  }

  switchTab(tab: string) {
    this.tabChange.emit(tab);
    if(tab === 'profile') {
      this.isDropdownOpen = false; // Close dropdown if profile is clicked
    }
  }

  logout() {
    this.logoutTrigger.emit();
  }
}