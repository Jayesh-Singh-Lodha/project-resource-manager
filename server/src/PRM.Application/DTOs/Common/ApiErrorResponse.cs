namespace PRM.Application.DTOs.Common;

/// <summary>
/// Standard error response format returned by the API for all error cases.
/// Consistent shape allows the console client to deserialize errors uniformly.
/// </summary>
public record ApiErrorResponse(
    int StatusCode,
    string Message,
    string[] Errors
);
