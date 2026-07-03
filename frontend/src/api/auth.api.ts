import apiClient from '../lib/axios';
import type { LoginRequest, LoginResponse, ChangePasswordRequest } from '../types';

/**
 * POST /api/auth/login
 */
export async function login(request: LoginRequest): Promise<LoginResponse> {
  const { data } = await apiClient.post<LoginResponse>('/api/auth/login', request);
  return data;
}

/**
 * POST /api/auth/change-password
 */
export async function changePassword(request: ChangePasswordRequest): Promise<void> {
  await apiClient.post('/api/auth/change-password', request);
}
