import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CitizenNavbarComponent } from '../citizen-navbar.component/citizen-navbar.component';
import { ChangePasswordComponent } from '../../../account/components/change-password.component/change-password.component';
import { CitizenService } from '../../services/citizen.service';
import { CitizenProfile, UpdateCitizenProfileRequest } from '../../models/citizen.model';

@Component({
  selector: 'app-citizen-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, CitizenNavbarComponent, ChangePasswordComponent],
  templateUrl: './citizen-profile.component.html'
})
export class CitizenProfileComponent implements OnInit {
  profileForm!: FormGroup;
  currentUserId: number | null = null; 
  actualCitizenId: number | null = null; 

  isLoading = false;
  isSaving = false;
  isEditing = false;
  successMessage = '';
  errorMessage = '';

  constructor(private fb: FormBuilder, private citizenService: CitizenService) {}

  ngOnInit() {
    this.initForm();
    this.extractUserIdFromToken();
    if (this.currentUserId) this.loadProfile();
  }

  initForm() {
    this.profileForm = this.fb.group({
      name: [{value: '', disabled: true}, Validators.required],
      email: [{value: '', disabled: true}, [Validators.required, Validators.email]],
      contactInfo: [{value: '', disabled: true}, Validators.required],
      address: [{value: '', disabled: true}, Validators.required],
      dateOfBirth: [{value: '', disabled: true}], 
      gender: [{value: '', disabled: true}]
    });
  }

  toggleEdit() {
    this.isEditing = !this.isEditing;
    this.errorMessage = '';
    this.successMessage = '';
    if (this.isEditing) {
      this.profileForm.get('name')?.enable();
      this.profileForm.get('email')?.enable();
      this.profileForm.get('contactInfo')?.enable();
      this.profileForm.get('address')?.enable();
    } else {
      this.profileForm.get('name')?.disable();
      this.profileForm.get('email')?.disable();
      this.profileForm.get('contactInfo')?.disable();
      this.profileForm.get('address')?.disable();
    }
  }

  extractUserIdFromToken() {
    const token = localStorage.getItem('token');
    if (token) {
      const payload = JSON.parse(atob(token.split('.')[1]));
      this.currentUserId = Number(payload.UserId || payload.sub || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier']);
    }
  }

  loadProfile() {
    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';
    
    this.citizenService.getProfile(this.currentUserId!).subscribe({
      // STRONGLY TYPED INTERFACE INSTEAD OF 'any'
      next: (data: CitizenProfile) => { 
        console.log('Loaded citizen profile data:', data);
        
        // Properly capture ID
        this.actualCitizenId = data.citizenId ?? null;

        const dob = data.dateOfBirth ? new Date(data.dateOfBirth).toISOString().split('T')[0] : '';
        
        this.profileForm.enable();
        this.profileForm.patchValue({
          name: data.name || '',
          email: data.email || '', // Will be blank if API doesn't send it, which is safe
          contactInfo: data.contactInfo || '',
          address: data.address || '',
          dateOfBirth: dob,
          gender: data.gender || ''
        });
        
        this.profileForm.get('dateOfBirth')?.disable();
        this.profileForm.get('gender')?.disable();
        if(!this.isEditing) {
            this.profileForm.get('name')?.disable();
            this.profileForm.get('email')?.disable();
            this.profileForm.get('contactInfo')?.disable();
            this.profileForm.get('address')?.disable();
        }
        
        this.isLoading = false;
      },
      error: (err) => {
        this.errorMessage = "Failed to load profile.";
        this.isLoading = false;
        console.error('Profile load error:', err);
      }
    });
  }

  onSubmit() {
    if (!this.actualCitizenId) {
        this.errorMessage = "Missing Citizen Profile ID. Cannot update.";
        return;
    }
    
    this.isSaving = true;
    this.errorMessage = '';
    this.successMessage = '';
    
    const formValue = this.profileForm.getRawValue();
    const payload: UpdateCitizenProfileRequest = {
      citizenId: this.actualCitizenId, // Ensure it's included
      name: formValue.name || '',
      email: formValue.email || '',
      contactInfo: formValue.contactInfo || '',
      address: formValue.address || '',
    };

    console.log('Submitting profile update payload:', payload);
    
    this.citizenService.updateProfile(this.actualCitizenId, payload).subscribe({
      next: () => {
        this.isSaving = false;
        this.successMessage = 'Profile updated successfully!';
        this.toggleEdit(); 
        localStorage.setItem('userName', payload.name); 
      },
      error: (err) => {
        this.isSaving = false;
        let errorMsg = 'Failed to update profile.';
        if (err.error) {
          if (typeof err.error === 'string') errorMsg = err.error;
          else if (err.error.message) errorMsg = err.error.message;
          else if (err.error.error) errorMsg = err.error.error;
        } else if (err.message) {
          errorMsg = err.message;
        }
        this.errorMessage = errorMsg;
        console.error('Profile update error:', err);
      }
    });
  }
}