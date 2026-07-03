using PRM.Core.Entities;

namespace PRM.Core.Interfaces;

/// <summary>
/// Repository contract for Timesheet and TimesheetEntry data access.
/// </summary>
public interface ITimesheetRepository
{
    Task<Timesheet?> GetByIdAsync(int id);
    Task<Timesheet?> GetByEmployeeAndWeekAsync(int userId, DateTime weekStartDate);
    Task<IReadOnlyList<Timesheet>> GetByEmployeeIdAsync(int userId);
    Task<IReadOnlyList<Timesheet>> GetTeamTimesheetsAsync(int managerId, DateTime weekStartDate);
    Task AddAsync(Timesheet timesheet);
    Task UpdateAsync(Timesheet timesheet);
    Task<IReadOnlyList<TimesheetEntry>> GetRecentEntriesByProjectAsync(int projectId, int limitWeeks);
}
