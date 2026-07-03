namespace PRM.Application.DTOs.Timesheets;

public record TimesheetEntryResponse(
    int Id,
    int ProjectId,
    string ProjectName,
    decimal HoursWorked,
    string? ActivityTags
);

public record TimesheetResponse(
    int Id,
    int UserId,
    string UserName,
    DateTime WeekStartDate,
    string Status,
    DateTime? SubmittedAt,
    List<TimesheetEntryResponse> Entries
);
