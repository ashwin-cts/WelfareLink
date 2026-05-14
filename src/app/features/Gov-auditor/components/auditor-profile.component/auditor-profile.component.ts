import { Component } from '@angular/core';

import { EditProfileComponent } from '../../../account/components/edit-profile.component/edit-profile.component'; // Adjust path if needed
import { ChangePasswordComponent } from '../../../account/components/change-password.component/change-password.component'; // Adjust path if needed
import { AuditorNavbarComponent } from '../auditor-navbar.component/auditor-navbar.component';

@Component({
  selector: 'app-auditor-profile',
  standalone: true,
  imports: [AuditorNavbarComponent, EditProfileComponent, ChangePasswordComponent],
  template: `
    <app-auditor-navbar></app-auditor-navbar>
    <div class="container py-4">
      <h2 class="mb-4">Auditor Profile Management</h2>
      <div class="row">
        <div class="col-md-6 mb-4">
          <app-edit-profile></app-edit-profile>
        </div>
        <div class="col-md-6 mb-4">
          <app-change-password></app-change-password>
        </div>
      </div>
    </div>
  `,
})
export class AuditorProfileComponent {}
