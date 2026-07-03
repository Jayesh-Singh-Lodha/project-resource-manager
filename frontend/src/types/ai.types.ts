// AI types — for AI assistant features

export interface AiSearchResponse {
  response: string;
}

export interface AiRiskSummaryResponse {
  summary: string;
}

export interface EmployeeDetailResponse {
  employee: import('./user.types').UserResponse;
  allocations: import('./allocation.types').AllocationResponse[];
  recentActivityTags: string[];
}
