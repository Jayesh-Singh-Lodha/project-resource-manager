// Project types — mirrors PRM.Application.DTOs.Projects

export interface ProjectResponse {
  id: number;
  name: string;
  description: string | null;
  startDate: string;
  endDate: string;
  status: string;
  managerId: number | null;
  managerName: string | null;
  totalStoryPoints: number;
  storyPointsCompleted: number;
  healthStatus: string;
  createdAt: string;
}

export interface CreateProjectRequest {
  name: string;
  description: string | null;
  startDate: string;
  endDate: string;
  status: string;
  managerId: number | null;
  totalStoryPoints: number;
}

export interface UpdateProjectRequest {
  name: string;
  description: string | null;
  startDate: string;
  endDate: string;
  status: string;
  managerId: number | null;
  totalStoryPoints: number;
}

export interface MilestoneResponse {
  id: number;
  projectId: number;
  title: string;
  dueDate: string;
  storyPoints: number;
  status: string;
}

export interface AddMilestoneRequest {
  title: string;
  dueDate: string;
  storyPoints: number;
}

export interface UpdateMilestoneStatusRequest {
  status: string;
}

export type ProjectStatus = 'Planned' | 'Active' | 'OnHold' | 'Completed';
export type MilestoneStatus = 'NotStarted' | 'InProgress' | 'Done';
export type HealthStatus = 'OnTrack' | 'Attention' | 'AtRisk';
