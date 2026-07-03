import apiClient from '../lib/axios';
import type { SystemConfigResponse, UpdateSystemConfigRequest } from '../types';

export async function getAllConfigs(): Promise<SystemConfigResponse[]> {
  const { data } = await apiClient.get<SystemConfigResponse[]>('/api/config');
  return data;
}

export async function getConfigByKey(key: string): Promise<SystemConfigResponse> {
  const { data } = await apiClient.get<SystemConfigResponse>(`/api/config/${key}`);
  return data;
}

export async function updateConfig(key: string, request: UpdateSystemConfigRequest): Promise<void> {
  await apiClient.put(`/api/config/${key}`, request);
}
