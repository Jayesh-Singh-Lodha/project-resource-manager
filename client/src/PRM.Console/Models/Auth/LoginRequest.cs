namespace PRM.Console.Models.Auth;

/// <summary>
/// Request payload for the login endpoint.
/// </summary>
public record LoginRequest(string Username, string Password);
