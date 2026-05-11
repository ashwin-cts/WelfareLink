import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';

import { AccountService } from '../../services/account.service';
import { UpdateProfileRequest } from '../../models/account.model';

@Component({
  selector: 'app-edit-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './edit-profile.component.html'
})
export class EditProfileComponent implements OnInit {
  profileForm: FormGroup;
  currentUserId: number | null = null;
  isLoading = false;
  isSaving = false;
  successMessage = '';
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private accountService: AccountService,
    private cdr: ChangeDetectorRef
  ) {
    this.profileForm = this.fb.group({
      fullName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]]
    });
  }

  ngOnInit() {
    this.extractUserIdFromToken();
    if (this.currentUserId) {
      this.loadProfile();
    }
  }

  extractUserIdFromToken() {
    const token = localStorage.getItem('token') || localStorage.getItem('jwt');
    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        const nameIdentifier = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
        this.currentUserId = Number(payload.UserId || payload.sub || nameIdentifier);
      } catch (e) {
        this.errorMessage = "Session error. Please log in again.";
      }
    }
  }

  loadProfile() {
    this.isLoading = true;
    this.accountService.getProfile(this.currentUserId!).subscribe({
      next: (data) => {
        this.profileForm.patchValue({
          fullName: data.fullName,
          email: data.email
        });
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.errorMessage = "Failed to load profile data.";
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  onSubmit() {
    if (this.profileForm.invalid || !this.currentUserId) return;

    this.isSaving = true;
    this.errorMessage = '';
    this.successMessage = '';

    const payload: UpdateProfileRequest = this.profileForm.value;

    this.accountService.updateProfile(this.currentUserId, payload).subscribe({
      next: (res) => {
        this.isSaving = false;
        this.successMessage = 'Profile updated successfully!';

        // Instantly update the name in the browser so the Navbar updates!
        if (payload.fullName) {
          localStorage.setItem('userName', payload.fullName);
        }
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isSaving = false;
        this.errorMessage = err.error?.Error || 'Failed to update profile.';
        this.cdr.detectChanges();
      }
    });
  }
}