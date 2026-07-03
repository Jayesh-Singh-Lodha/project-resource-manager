using PRM.Core.Entities;

namespace PRM.Core.Interfaces;

/// <summary>
/// Contract for JWT token generation.
/// Implemented in PRM.Infrastructure.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a signed JWT for the given user.
    /// Token includes claims: sub (user ID), role, name, jti (unique token ID).
    /// </summary>
    string GenerateToken(User user);
}
