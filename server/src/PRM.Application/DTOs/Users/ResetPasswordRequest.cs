namespace PRM.Application.DTOs.Users;

public record ResetPasswordRequest(
    string NewTemporaryPassword
);
