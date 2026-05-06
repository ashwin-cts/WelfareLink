// --- REQUEST PAYLOADS (Data going TO the server) ---
export interface LoginCredentials {
  username: string;
  password?: string;
  userType: string;
}

export interface RegisterCitizenRequest {
  username: string;
  password?: string;
  email?: string;
  fullName?: string;
  // Add any other specific fields your Citizen registration needs here!
}

// --- RESPONSE TYPES (Data coming FROM the server) ---
export interface AuthResponse {
  token?: string;
  Token?: string; // Handling C# capitalization quirks
  role?: string;
  Role?: string;
  username?: string;
}

export interface AuthErrorResponse {
  error?: string;
  Error?: string;
  errors?: { [key: string]: string[] };
}