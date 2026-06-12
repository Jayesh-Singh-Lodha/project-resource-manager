namespace PRM.Application.DTOs.Auth;

/// <summary>
/// Request payload for the login endpoint.
/// </summary>
public record LoginRequest(string Username, string Password);
