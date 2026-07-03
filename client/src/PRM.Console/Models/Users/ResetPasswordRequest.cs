namespace PRM.Console.Models.Users;

public record ResetPasswordRequest(
    string NewTemporaryPassword
);
