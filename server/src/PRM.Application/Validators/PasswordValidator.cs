using PRM.Core.Constants;

namespace PRM.Application.Validators;

/// <summary>
/// Validates password strength requirements.
/// Rules: minimum 8 characters, at least one uppercase letter, at least one digit.
/// </summary>
public static class PasswordValidator
{
    /// <summary>
    /// Validates that the password meets all strength requirements.
    /// Returns a list of validation error messages (empty if valid).
    /// </summary>
    public static List<string> Validate(string password)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(password))
        {
            errors.Add("Password cannot be empty.");
            return errors;
        }

        if (password.Length < AppConstants.PasswordMinLength)
        {
            errors.Add($"Password must be at least {AppConstants.PasswordMinLength} characters.");
        }

        if (!password.Any(char.IsUpper))
        {
            errors.Add("Password must contain at least one uppercase letter.");
        }

        if (!password.Any(char.IsDigit))
        {
            errors.Add("Password must contain at least one digit.");
        }

        return errors;
    }
}
