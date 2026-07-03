// Allocation types — mirrors PRM.Application.DTOs.Allocations

export interface AllocationResponse {
  id: number;
  userId: number;
  userName: string;
  projectId: number;
  projectName: string;
  utilisationPercent: number;
  fromDate: string;
  toDate: string;
}

export interface CreateAllocationRequest {
  userId: number;
  projectId: number;
  utilisationPercent: number;
  fromDate: string;
  toDate: string;
}
