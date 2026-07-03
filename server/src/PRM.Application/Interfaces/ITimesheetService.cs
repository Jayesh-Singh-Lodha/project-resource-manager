using PRM.Application.DTOs.Timesheets;

namespace PRM.Application.Interfaces;

public interface ITimesheetService
{
    Task<TimesheetResponse> SubmitTimesheetAsync(SubmitTimesheetRequest request);
    Task<IReadOnlyList<TimesheetResponse>> GetTimesheetsByEmployeeIdAsync(int userId);
    Task<IReadOnlyList<TimesheetResponse>> GetTeamTimesheetsAsync(int managerId, DateTime weekStartDate);
    Task<TimesheetResponse?> GetTimesheetByIdAsync(int id);
    Task UpdateTimesheetStatusAsync(int id, string status);
    Task<TimesheetResponse?> GetLastWeekTimesheetAsync(int userId);
    Task RestoreTimesheetAccessAsync(int employeeId, int managerId);
}
