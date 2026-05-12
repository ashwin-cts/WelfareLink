import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

// 1. Import the Welfare Officer Navbar
import { WelfareApplicationNavbarComponent } from '../welfare-application-navbar.component/welfare-application-navbar.component';

// 2. Import the Reusable Account Components (adjust the relative paths if necessary)
import { ChangePasswordComponent } from '../../../account/components/change-password.component/change-password.component';
import { EditProfileComponent } from '../../../account/components/edit-profile.component/edit-profile.component';

@Component({
  selector: 'app-welfare-officer-profile',
  standalone: true,
  imports: [
    CommonModule, 
    RouterModule, 
    WelfareApplicationNavbarComponent, 
    ChangePasswordComponent, 
    EditProfileComponent
  ],
  templateUrl: './welfare-officer-profile.component.html',
  styleUrls: ['./welfare-officer-profile.component.css']
})
export class WelfareOfficerProfileComponent { }