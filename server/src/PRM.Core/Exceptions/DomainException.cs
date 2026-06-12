namespace PRM.Core.Exceptions;

/// <summary>
/// Base exception for all PRM domain errors.
/// Subclassed for specific error types (credentials, inactive account, etc.).
/// Caught by the API's global exception handling middleware.
/// </summary>
public class DomainException : Exception
{
    public string ErrorCode { get; }

    public DomainException(string message, string errorCode = "DOMAIN_ERROR")
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public DomainException(string message, string errorCode, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}
