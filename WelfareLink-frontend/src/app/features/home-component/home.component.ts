import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Login } from '../auth/login/components/login'; // Ensure this path correctly points to your login component!

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterModule, Login], // Added Login here
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent { 
  // State variable to track if the modal is open or closed
  isLoginVisible = false;

  openLogin() {
    this.isLoginVisible = true;
  }

  closeLogin() {
    this.isLoginVisible = false;
  }
}