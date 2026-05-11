import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';

@Component({
  selector: 'app-auditor-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './auditor-navbar.component.html',
  styleUrls: ['./auditor-navbar.component.css']
})
export class AuditorNavbarComponent implements OnInit {
  userName: string = 'Auditor';

  constructor(private router: Router) {}

  ngOnInit(): void {
    // Attempt to get the user's name from localStorage for a personalized greeting
    const storedName = localStorage.getItem('userName');
    if (storedName) {
      this.userName = storedName;
    }
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('jwt');
    localStorage.removeItem('currentUser');
    localStorage.removeItem('userName');
    this.router.navigate(['/login']);
  }
}