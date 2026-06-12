namespace PRM.Application.DTOs.Timesheets;

public record TimesheetEntryDto(
    int ProjectId,
    decimal HoursWorked,
    string? ActivityTags
);

public record SubmitTimesheetRequest(
    int UserId,
    DateTime WeekStartDate,
    List<TimesheetEntryDto> Entries
);
