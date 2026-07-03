namespace PRM.Core.Exceptions;

/// <summary>
/// Thrown when login credentials (username or password) are invalid.
/// Maps to HTTP 401 Unauthorized in the API middleware.
/// </summary>
public class InvalidCredentialsException : DomainException
{
    public InvalidCredentialsException()
        : base("Invalid username or password.", "INVALID_CREDENTIALS")
    {
    }
}
