// src/app/features/account/models/account.model.ts

export interface AccountProfile {
    fullName: string;
    email: string;
}
export interface UserProfile {
    userId?: number;
    username: string;
    fullName?: string;
    email?: string;
    role?: string;
    isActive?: boolean;
}

export interface UpdateProfileRequest {
    fullName?: string;
    email?: string;
}

export interface ChangePasswordRequest {
    currentPassword: string;
    newPassword: string;
}

// Reusable standard API response
export interface ApiResponse {
    message?: string;
    success?: boolean;
}