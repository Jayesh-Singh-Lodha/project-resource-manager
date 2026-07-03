import apiClient from '../lib/axios';
import type {
  ProjectResponse,
  CreateProjectRequest,
  UpdateProjectRequest,
  MilestoneResponse,
  AddMilestoneRequest,
  UpdateMilestoneStatusRequest,
} from '../types';

export async function createProject(request: CreateProjectRequest): Promise<ProjectResponse> {
  const { data } = await apiClient.post<ProjectResponse>('/api/projects', request);
  return data;
}

export async function getAllProjects(): Promise<ProjectResponse[]> {
  const { data } = await apiClient.get<ProjectResponse[]>('/api/projects');
  return data;
}

export async function getProjectById(id: number): Promise<ProjectResponse> {
  const { data } = await apiClient.get<ProjectResponse>(`/api/projects/${id}`);
  return data;
}

export async function updateProject(id: number, request: UpdateProjectRequest): Promise<void> {
  await apiClient.put(`/api/projects/${id}`, request);
}

export async function addMilestone(projectId: number, request: AddMilestoneRequest): Promise<void> {
  await apiClient.post(`/api/projects/${projectId}/milestones`, request);
}

export async function updateMilestoneStatus(milestoneId: number, request: UpdateMilestoneStatusRequest): Promise<void> {
  await apiClient.put(`/api/projects/milestones/${milestoneId}/status`, request);
}

export async function getMilestonesByProjectId(projectId: number): Promise<MilestoneResponse[]> {
  const { data } = await apiClient.get<MilestoneResponse[]>(`/api/projects/${projectId}/milestones`);
  return data;
}
