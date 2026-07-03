namespace PRM.Application.DTOs.Users;

public record UpdateUserRequest(
    string FullName,
    string? Department,
    string Role
);
