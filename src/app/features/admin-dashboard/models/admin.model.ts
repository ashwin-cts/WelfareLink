export interface UserProfile {
  userId?: number;
  username: string;
  fullName?: string;
  email?: string;
  role?: string;
  isActive?: boolean;
}

export interface SystemLog {
  timestamp: string;
  userName: string;
  action: string;
  entityType: string;
  entityId: string;
  description: string;
  ipAddress: string;
}

export interface PaginatedLogs {
  items?: SystemLog[];
  data?: SystemLog[];
  records?: SystemLog[];
  pageNumber?: number;
  currentPage?: number;
  totalPages?: number;
}


export interface CreateUserRequest {
  username: string;
  password?: string;
  role: string;
  fullName?: string;
  email?: string;
}

export interface UpdateProfileRequest {
  fullName?: string;
  email?: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

// --- RESPONSE TYPES (Data coming FROM the server) ---
// Use this for endpoints that just return a simple success message
export interface ApiResponse {
  message?: string;
  success?: boolean;
}