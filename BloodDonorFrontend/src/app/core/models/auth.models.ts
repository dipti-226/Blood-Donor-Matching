export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  userId: string;
  email: string;
  roles: string[];
  accessToken: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  confirmPassword: string;
  phoneNumber?: string | null;
  requestedRole: string;
}

export interface RegisterResponse {
  userId: string;
  email: string;
  role: string;
}