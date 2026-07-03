import apiClient from '../lib/axios';
import type {
  UserResponse,
  CreateUserRequest,
  CreateUserResponse,
  UpdateUserRequest,
  ResetPasswordRequest,
  AddSkillRequest,
  UpdateSkillRequest,
} from '../types';

// --- Admin user management endpoints (api/admin) ---

export async function createUser(request: CreateUserRequest): Promise<CreateUserResponse> {
  const { data } = await apiClient.post<CreateUserResponse>('/api/admin/users', request);
  return data;
}

export async function getAllUsers(): Promise<UserResponse[]> {
  const { data } = await apiClient.get<UserResponse[]>('/api/admin/users');
  return data;
}

export async function updateUser(id: number, request: UpdateUserRequest): Promise<void> {
  await apiClient.put(`/api/admin/users/${id}`, request);
}

export async function deactivateUser(id: number): Promise<void> {
  await apiClient.post(`/api/admin/users/${id}/deactivate`);
}

export async function reactivateUser(id: number): Promise<void> {
  await apiClient.post(`/api/admin/users/${id}/reactivate`);
}

export async function resetPassword(id: number, request: ResetPasswordRequest): Promise<void> {
  await apiClient.post(`/api/admin/users/${id}/reset-password`, request);
}

export async function addSkill(userId: number, request: AddSkillRequest): Promise<void> {
  await apiClient.post(`/api/admin/users/${userId}/skills`, request);
}

export async function updateSkillProficiency(userId: number, request: UpdateSkillRequest): Promise<void> {
  await apiClient.put(`/api/admin/users/${userId}/skills`, request);
}

export async function removeSkill(userId: number, skillName: string): Promise<void> {
  await apiClient.delete(`/api/admin/users/${userId}/skills/${encodeURIComponent(skillName)}`);
}

export async function assignManager(userId: number, managerId: number | null): Promise<void> {
  await apiClient.post(`/api/admin/users/${userId}/manager?managerId=${managerId ?? ''}`);
}
