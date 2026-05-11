import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';

@Component({
  selector: 'app-citizen-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './citizen-navbar.component.html'
})
export class CitizenNavbarComponent implements OnInit {
  userName: string = 'Citizen';

  constructor(private router: Router) {}

  ngOnInit(): void {
    const storedName = localStorage.getItem('userName');
    if (storedName) {
      this.userName = storedName;
    }
  }

  logout(): void {
    localStorage.clear();
    this.router.navigate(['/login']);
  }
}