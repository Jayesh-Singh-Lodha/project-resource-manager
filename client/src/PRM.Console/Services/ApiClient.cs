using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using PRM.Console.Models.Common;

namespace PRM.Console.Services;

/// <summary>
/// HTTP client wrapper for communicating with the PRM API server.
/// Manages the JWT token in-memory and attaches it to all authenticated requests.
/// </summary>
public class ApiClient
{
    private readonly HttpClient _httpClient;
    private string? _token;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// The currently stored JWT token. Null if not logged in.
    /// </summary>
    public string? Token
    {
        get => _token;
        set
        {
            _token = value;
            if (value is not null)
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", value);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }
    }

    /// <summary>
    /// Whether the client currently has a valid token set.
    /// </summary>
    public bool IsAuthenticated => _token is not null;

    /// <summary>
    /// Sends a POST request with a JSON body and deserializes the response.
    /// Throws ApiException on non-success status codes.
    /// </summary>
    public async Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest body)
    {
        var response = await _httpClient.PostAsJsonAsync(endpoint, body);

        if (!response.IsSuccessStatusCode)
        {
            await ThrowApiExceptionAsync(response);
        }

        var result = await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions);
        return result ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Sends a POST request with a JSON body. Does not expect a response body (204 No Content).
    /// Throws ApiException on non-success status codes.
    /// </summary>
    public async Task PostAsync<TRequest>(string endpoint, TRequest body)
    {
        var response = await _httpClient.PostAsJsonAsync(endpoint, body);

        if (!response.IsSuccessStatusCode)
        {
            await ThrowApiExceptionAsync(response);
        }
    }

    /// <summary>
    /// Sends a PUT request with a JSON body. Does not expect a response body.
    /// Throws ApiException on non-success status codes.
    /// </summary>
    public async Task PutAsync<TRequest>(string endpoint, TRequest body)
    {
        var response = await _httpClient.PutAsJsonAsync(endpoint, body);

        if (!response.IsSuccessStatusCode)
        {
            await ThrowApiExceptionAsync(response);
        }
    }

    /// <summary>
    /// Sends a DELETE request. Does not expect a response body.
    /// Throws ApiException on non-success status codes.
    /// </summary>
    public async Task DeleteAsync(string endpoint)
    {
        var response = await _httpClient.DeleteAsync(endpoint);

        if (!response.IsSuccessStatusCode)
        {
            await ThrowApiExceptionAsync(response);
        }
    }

    /// <summary>
    /// Sends a GET request and deserializes the response.
    /// Throws ApiException on non-success status codes.
    /// </summary>
    public async Task<TResponse> GetAsync<TResponse>(string endpoint)
    {
        var response = await _httpClient.GetAsync(endpoint);

        if (!response.IsSuccessStatusCode)
        {
            await ThrowApiExceptionAsync(response);
        }

        var result = await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions);
        return result ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Clears the stored token and removes the Authorization header.
    /// Used on logout.
    /// </summary>
    public void ClearToken()
    {
        Token = null;
    }

    private static async Task ThrowApiExceptionAsync(HttpResponseMessage response)
    {
        string message;
        string[] errors;

        try
        {
            var errorResponse = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions);
            message = errorResponse?.Message ?? "An error occurred.";
            errors = errorResponse?.Errors ?? Array.Empty<string>();
        }
        catch
        {
            message = $"Server returned {(int)response.StatusCode} {response.StatusCode}.";
            errors = Array.Empty<string>();
        }

        throw new ApiException(message, (int)response.StatusCode, errors);
    }
}

/// <summary>
/// Exception thrown when the API returns a non-success status code.
/// Contains the error message, HTTP status code, and any validation errors.
/// </summary>
public class ApiException : Exception
{
    public int StatusCode { get; }
    public string[] Errors { get; }

    public ApiException(string message, int statusCode, string[] errors)
        : base(message)
    {
        StatusCode = statusCode;
        Errors = errors;
    }
}
