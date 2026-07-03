import apiClient from '../lib/axios';
import type { AllocationResponse, CreateAllocationRequest } from '../types';

export async function getAllAllocations(): Promise<AllocationResponse[]> {
  const { data } = await apiClient.get<AllocationResponse[]>('/api/allocations');
  return data;
}

export async function createAllocation(request: CreateAllocationRequest): Promise<AllocationResponse> {
  const { data } = await apiClient.post<AllocationResponse>('/api/manager/allocations', request);
  return data;
}

export async function endAllocation(id: number): Promise<void> {
  await apiClient.delete(`/api/allocations/${id}`);
}

export async function getProjectAllocations(projectId: number): Promise<AllocationResponse[]> {
  const { data } = await apiClient.get<AllocationResponse[]>(`/api/manager/projects/${projectId}/allocations`);
  return data;
}

export async function getMyAllocations(): Promise<AllocationResponse[]> {
  const { data } = await apiClient.get<AllocationResponse[]>('/api/employee/allocations');
  return data;
}
