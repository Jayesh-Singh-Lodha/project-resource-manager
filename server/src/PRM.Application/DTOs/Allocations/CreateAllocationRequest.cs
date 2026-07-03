namespace PRM.Application.DTOs.Allocations;

public record CreateAllocationRequest(
    int UserId,
    int ProjectId,
    int UtilisationPercent,
    DateTime FromDate,
    DateTime ToDate
);
