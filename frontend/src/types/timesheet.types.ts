// Timesheet types — mirrors PRM.Application.DTOs.Timesheets

export interface TimesheetEntryResponse {
  id: number;
  projectId: number;
  projectName: string;
  hoursWorked: number;
  activityTags: string | null;
}

export interface TimesheetResponse {
  id: number;
  userId: number;
  userName: string;
  weekStartDate: string;
  status: string;
  submittedAt: string | null;
  entries: TimesheetEntryResponse[];
}

export interface TimesheetEntryDto {
  projectId: number;
  hoursWorked: number;
  activityTags: string | null;
}

export interface SubmitTimesheetRequest {
  userId: number;
  weekStartDate: string;
  entries: TimesheetEntryDto[];
}

export type TimesheetStatus = 'Submitted' | 'Missed' | 'Pending';
