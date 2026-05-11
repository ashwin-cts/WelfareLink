import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-resource-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './resource-navbar.component.html',
  styleUrls: ['./resource-navbar.component.css']
})
export class ResourceNavbarComponent implements OnInit {
  private router = inject(Router);

  userName = signal<string>('Loading...');
  userRole = signal<string>('Guest');

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