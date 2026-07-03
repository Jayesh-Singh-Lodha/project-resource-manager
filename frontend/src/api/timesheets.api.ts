import apiClient from '../lib/axios';
import type { TimesheetResponse, SubmitTimesheetRequest } from '../types';

export async function submitTimesheet(request: SubmitTimesheetRequest): Promise<TimesheetResponse> {
  const { data } = await apiClient.post<TimesheetResponse>('/api/employee/timesheets', request);
  return data;
}

export async function getMyTimesheets(): Promise<TimesheetResponse[]> {
  const { data } = await apiClient.get<TimesheetResponse[]>('/api/employee/timesheets');
  return data;
}

export async function getLastWeekTimesheetStatus(): Promise<TimesheetResponse | null> {
  const response = await apiClient.get('/api/employee/timesheets/last-week-status');
  if (response.status === 204) return null;
  return response.data;
}

export async function getTeamTimesheets(weekStartDate: string): Promise<TimesheetResponse[]> {
  const { data } = await apiClient.get<TimesheetResponse[]>('/api/manager/timesheets', {
    params: { weekStartDate },
  });
  return data;
}

export async function updateTimesheetStatus(timesheetId: number, status: string): Promise<void> {
  await apiClient.put(`/api/manager/timesheets/${timesheetId}/status`, JSON.stringify(status), {
    headers: { 'Content-Type': 'application/json' },
  });
}

export async function restoreTimesheetAccess(employeeId: number): Promise<void> {
  await apiClient.post(`/api/manager/employees/${employeeId}/timesheets/restore`);
}
