namespace PRM.Console.Models.Allocations;

public record AllocationResponse(
    int Id,
    int UserId,
    string UserName,
    int ProjectId,
    string ProjectName,
    int UtilisationPercent,
    DateTime FromDate,
    DateTime ToDate
);
