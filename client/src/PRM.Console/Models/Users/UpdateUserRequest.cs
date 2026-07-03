namespace PRM.Console.Models.Users;

public record UpdateUserRequest(
    string FullName,
    string? Department,
    string Role
);
