using PRM.Application.DTOs.Timesheets;
using PRM.Application.Interfaces;
using PRM.Core.Constants;
using PRM.Core.Entities;
using PRM.Core.Enums;
using PRM.Core.Exceptions;
using PRM.Core.Interfaces;

namespace PRM.Application.Services;

public class TimesheetService : ITimesheetService
{
    private readonly ITimesheetRepository _timesheetRepository;
    private readonly IAllocationRepository _allocationRepository;
    private readonly IUserRepository _userRepository;
    private readonly ISystemConfigRepository _configRepository;

    public TimesheetService(
        ITimesheetRepository timesheetRepository,
        IAllocationRepository allocationRepository,
        IUserRepository userRepository,
        ISystemConfigRepository configRepository)
    {
        _timesheetRepository = timesheetRepository;
        _allocationRepository = allocationRepository;
        _userRepository = userRepository;
        _configRepository = configRepository;
    }

    public async Task<TimesheetResponse> SubmitTimesheetAsync(SubmitTimesheetRequest request)
    {
        // 1. Validate no future week
        var today = DateTime.UtcNow.Date;
        var weekStart = request.WeekStartDate.Date;
        if (weekStart > today)
        {
            throw new DomainException("Cannot submit timesheets for future weeks.", "FUTURE_TIMESHEET");
        }

        // 2. Validate user
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user is null || !user.IsActive)
        {
            throw new DomainException("Employee not found or is inactive.", "EMPLOYEE_NOT_FOUND");
        }

        if (user.IsTimesheetFrozen)
        {
            throw new DomainException("Timesheet submission access is frozen due to missing timesheets. Please contact your manager.", "TIMESHEET_FROZEN");
        }

        // 3. Load configurations
        var maxHoursConfig = await _configRepository.GetByKeyAsync(AppConstants.ConfigKeyMaxWeeklyHours);
        var maxWeeklyHours = maxHoursConfig is not null && decimal.TryParse(maxHoursConfig.Value, out var val)
            ? val
            : AppConstants.DefaultMaxWeeklyHours;

        // 4. Validate duplicate timesheet
        var existing = await _timesheetRepository.GetByEmployeeAndWeekAsync(request.UserId, weekStart);
        if (existing is not null && existing.Status == TimesheetStatus.Submitted)
        {
            throw new DomainException("Timesheet already submitted for this week.", "DUPLICATE_TIMESHEET");
        }

        // 5. Validate total hours
        var totalHours = request.Entries.Sum(e => e.HoursWorked);
        if (totalHours > maxWeeklyHours)
        {
            throw new DomainException($"Total hours worked ({totalHours}) exceeds the maximum weekly hours limit of {maxWeeklyHours}.", "EXCEEDS_MAX_WEEKLY_HOURS");
        }

        // 6. Validate project allocation hours
        var weekEnd = weekStart.AddDays(6);
        var allocations = await _allocationRepository.GetOverlappingAllocationsAsync(request.UserId, weekStart, weekEnd);

        foreach (var entry in request.Entries)
        {
            if (entry.HoursWorked < 0)
            {
                throw new DomainException("Hours worked cannot be negative.", "INVALID_HOURS");
            }

            var alloc = allocations.FirstOrDefault(a => a.ProjectId == entry.ProjectId);
            var maxProjectHours = alloc is not null
                ? (alloc.UtilisationPercent / 100.0m) * maxWeeklyHours
                : 0.0m;

            if (entry.HoursWorked > maxProjectHours)
            {
                throw new DomainException(
                    $"Hours worked ({entry.HoursWorked}) for Project ID {entry.ProjectId} exceeds the allocation limit of {maxProjectHours} hours ({alloc?.UtilisationPercent ?? 0}% utilisation).",
                    "EXCEEDS_ALLOCATION_LIMIT");
            }
        }

        // 7. Save timesheet
        var timesheet = existing ?? new Timesheet
        {
            UserId = request.UserId,
            WeekStartDate = weekStart
        };

        timesheet.Status = TimesheetStatus.Submitted;
        timesheet.SubmittedAt = DateTime.UtcNow;
        timesheet.Entries.Clear();

        foreach (var entry in request.Entries)
        {
            timesheet.Entries.Add(new TimesheetEntry
            {
                ProjectId = entry.ProjectId,
                HoursWorked = entry.HoursWorked,
                ActivityTags = entry.ActivityTags?.Trim()
            });
        }

        if (existing is null)
        {
            await _timesheetRepository.AddAsync(timesheet);
        }
        else
        {
            await _timesheetRepository.UpdateAsync(timesheet);
        }

        var saved = await _timesheetRepository.GetByIdAsync(timesheet.Id);
        return MapToResponse(saved!);
    }

    public async Task<IReadOnlyList<TimesheetResponse>> GetTimesheetsByEmployeeIdAsync(int userId)
    {
        var timesheets = await _timesheetRepository.GetByEmployeeIdAsync(userId);
        return timesheets.Select(MapToResponse).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<TimesheetResponse>> GetTeamTimesheetsAsync(int managerId, DateTime weekStartDate)
    {
        var timesheets = await _timesheetRepository.GetTeamTimesheetsAsync(managerId, weekStartDate);
        return timesheets.Select(MapToResponse).ToList().AsReadOnly();
    }

    public async Task<TimesheetResponse?> GetTimesheetByIdAsync(int id)
    {
        var timesheet = await _timesheetRepository.GetByIdAsync(id);
        if (timesheet is null) return null;
        return MapToResponse(timesheet);
    }

    public async Task UpdateTimesheetStatusAsync(int id, string status)
    {
        var timesheet = await _timesheetRepository.GetByIdAsync(id);
        if (timesheet is null)
        {
            throw new DomainException("Timesheet not found.", "TIMESHEET_NOT_FOUND");
        }

        if (!Enum.TryParse<TimesheetStatus>(status, true, out var parsedStatus))
        {
            throw new DomainException("Invalid timesheet status.", "INVALID_STATUS");
        }

        timesheet.Status = parsedStatus;
        await _timesheetRepository.UpdateAsync(timesheet);
    }

    public async Task RestoreTimesheetAccessAsync(int employeeId, int managerId)
    {
        var employee = await _userRepository.GetByIdAsync(employeeId);
        if (employee is null)
        {
            throw new DomainException("Employee not found.", "EMPLOYEE_NOT_FOUND");
        }

        if (employee.ManagerId != managerId)
        {
            throw new DomainException("Only the direct manager can restore timesheet access.", "UNAUTHORIZED_MANAGER");
        }

        if (!employee.IsTimesheetFrozen)
        {
            throw new DomainException("Employee timesheet access is not frozen.", "NOT_FROZEN");
        }

        employee.IsTimesheetFrozen = false;
        await _userRepository.UpdateAsync(employee);

        // Find any missed timesheets and reset reminder count so the scheduler doesn't instantly refreeze
        var timesheets = await _timesheetRepository.GetByEmployeeIdAsync(employeeId);
        var missedTimesheets = timesheets.Where(t => t.Status == TimesheetStatus.Missed).ToList();
        
        foreach (var ts in missedTimesheets)
        {
            if (ts.ReminderCount >= 3)
            {
                ts.ReminderCount = 0; // Reset so they have a grace period, or we could just set to 2
                await _timesheetRepository.UpdateAsync(ts);
            }
        }
    }
    public async Task<TimesheetResponse?> GetLastWeekTimesheetAsync(int userId)
    {
        var diff = (7 + (DateTime.UtcNow.DayOfWeek - DayOfWeek.Monday)) % 7;
        var currentWeekStart = DateTime.UtcNow.AddDays(-1 * diff).Date;
        var lastWeekStart = currentWeekStart.AddDays(-7);

        var timesheet = await _timesheetRepository.GetByEmployeeAndWeekAsync(userId, lastWeekStart);
        if (timesheet is null) return null;

        // Re-fetch with full includes for mapping
        var full = await _timesheetRepository.GetByIdAsync(timesheet.Id);
        return full is not null ? MapToResponse(full) : null;
    }

    private static TimesheetResponse MapToResponse(Timesheet t)
    {
        return new TimesheetResponse(
            Id: t.Id,
            UserId: t.UserId,
            UserName: t.User?.FullName ?? string.Empty,
            WeekStartDate: t.WeekStartDate,
            Status: t.Status.ToString(),
            SubmittedAt: t.SubmittedAt,
            Entries: t.Entries.Select(e => new TimesheetEntryResponse(
                Id: e.Id,
                ProjectId: e.ProjectId,
                ProjectName: e.Project?.Name ?? string.Empty,
                HoursWorked: e.HoursWorked,
                ActivityTags: e.ActivityTags
            )).ToList()
        );
    }
}
