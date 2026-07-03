// User types — mirrors PRM.Application.DTOs.Users

export interface UserResponse {
  id: number;
  username: string;
  email: string;
  fullName: string;
  role: string;
  department: string | null;
  status: string;
  isActive: boolean;
  forcePasswordChange: boolean;
  isTimesheetFrozen: boolean;
  createdAt: string;
  managerId: number | null;
  skills: string[] | null;
  currentUtilisationPercent: number;
}

export interface CreateUserRequest {
  username: string;
  email: string;
  fullName: string;
  role: string;
  department?: string | null;
}

export interface CreateUserResponse {
  id: number;
  username: string;
  email: string;
  fullName: string;
  role: string;
  department: string | null;
  temporaryPassword: string;
  createdAt: string;
}

export interface UpdateUserRequest {
  fullName: string;
  department: string | null;
  role: string;
}

export interface ResetPasswordRequest {
  newTemporaryPassword: string;
}

export interface AddSkillRequest {
  skillName: string;
  category: string;
  proficiencyLevel: string;
}

export interface UpdateSkillRequest {
  skillName: string;
  proficiencyLevel: string;
}
