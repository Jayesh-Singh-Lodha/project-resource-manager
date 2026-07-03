import apiClient from '../lib/axios';
import type { UserResponse, AiSearchResponse, AiRiskSummaryResponse, EmployeeDetailResponse } from '../types';

export async function getTeam(): Promise<UserResponse[]> {
  const { data } = await apiClient.get<UserResponse[]>('/api/manager/team');
  return data;
}

export async function getManagedProjects() {
  const { data } = await apiClient.get('/api/manager/projects');
  return data;
}

export async function searchResources(criteria: string): Promise<AiSearchResponse> {
  const { data } = await apiClient.get<AiSearchResponse>('/api/manager/allocations/search', {
    params: { criteria },
  });
  return data;
}

export async function buildTeam(requirements: string): Promise<AiSearchResponse> {
  const { data } = await apiClient.get<AiSearchResponse>('/api/manager/allocations/build-team', {
    params: { requirements },
  });
  return data;
}

export async function getProjectRiskSummary(projectId: number): Promise<AiRiskSummaryResponse> {
  const { data } = await apiClient.get<AiRiskSummaryResponse>(`/api/manager/projects/${projectId}/risk-summary`);
  return data;
}

export async function getEmployeeDetail(employeeId: number): Promise<EmployeeDetailResponse> {
  const { data } = await apiClient.get<EmployeeDetailResponse>(`/api/manager/employees/${employeeId}/detail`);
  return data;
}
