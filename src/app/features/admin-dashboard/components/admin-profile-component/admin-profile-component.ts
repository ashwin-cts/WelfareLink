import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

// Import Admin Navbar
import { AdminNavbarComponent } from '../admin-navbar/admin-navbar'; 
import { ChangePasswordComponent } from '../../../account/components/change-password.component/change-password.component';
import { EditProfileComponent } from '../../../account/components/edit-profile.component/edit-profile.component';

@Component({
  selector: 'app-admin-profile',
  standalone: true,
  imports: [CommonModule, AdminNavbarComponent, ChangePasswordComponent, EditProfileComponent],
  templateUrl: './admin-profile-component.html', // <--- Points to the HTML file
  styleUrls: ['./admin-profile-component.css']   // <--- Points to the CSS file
})
export class AdminProfileComponent { }