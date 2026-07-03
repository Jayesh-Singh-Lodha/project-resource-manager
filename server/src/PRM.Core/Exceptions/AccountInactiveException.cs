namespace PRM.Core.Exceptions;

/// <summary>
/// Thrown when a deactivated user attempts to log in.
/// Maps to HTTP 403 Forbidden in the API middleware.
/// </summary>
public class AccountInactiveException : DomainException
{
    public AccountInactiveException()
        : base("This account has been deactivated.", "ACCOUNT_INACTIVE")
    {
    }
}
