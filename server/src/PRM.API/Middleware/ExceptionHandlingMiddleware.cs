using System.Net;
using System.Text.Json;
using PRM.Application.DTOs.Common;
using PRM.Core.Exceptions;

namespace PRM.API.Middleware;

/// <summary>
/// Global exception handling middleware.
/// Catches domain exceptions and maps them to appropriate HTTP status codes.
/// Returns a consistent ApiErrorResponse JSON for all error cases.
/// Prevents stack trace leaking in production.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message, errors) = exception switch
        {
            InvalidCredentialsException ex =>
                (HttpStatusCode.Unauthorized, ex.Message, Array.Empty<string>()),

            AccountInactiveException ex =>
                (HttpStatusCode.Forbidden, ex.Message, Array.Empty<string>()),

            DomainException ex => MapDomainException(ex),

            _ => (HttpStatusCode.InternalServerError,
                  "An unexpected error occurred.",
                  Array.Empty<string>())
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning("Domain exception: {ErrorType} - {Message}",
                exception.GetType().Name, exception.Message);
        }

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = new ApiErrorResponse(
            StatusCode: (int)statusCode,
            Message: message,
            Errors: errors
        );

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private static (HttpStatusCode, string, string[]) MapDomainException(DomainException ex)
    {
        var errors = Array.Empty<string>();

        if (ex.Data.Contains("Errors") && ex.Data["Errors"] is List<string> validationErrors)
        {
            errors = validationErrors.ToArray();
        }

        return ex.ErrorCode switch
        {
            "PASSWORD_MISMATCH" => (HttpStatusCode.BadRequest, ex.Message, errors),
            "WEAK_PASSWORD" => (HttpStatusCode.BadRequest, ex.Message, errors),
            "USER_NOT_FOUND" => (HttpStatusCode.NotFound, ex.Message, errors),
            _ => (HttpStatusCode.BadRequest, ex.Message, errors)
        };
    }
}
