// Auth types — mirrors PRM.Application.DTOs.Auth

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  role: UserRole;
  forcePasswordChange: boolean;
  fullName: string;
}

export interface ChangePasswordRequest {
  newPassword: string;
  confirmPassword: string;
}

export type UserRole = 'Admin' | 'Manager' | 'Employee';
