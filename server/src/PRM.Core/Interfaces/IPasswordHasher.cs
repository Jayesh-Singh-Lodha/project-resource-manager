namespace PRM.Core.Interfaces;

/// <summary>
/// Contract for password hashing and verification.
/// Wraps BCrypt operations. Implemented in PRM.Infrastructure.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a plain-text password using BCrypt.
    /// </summary>
    string Hash(string plainPassword);

    /// <summary>
    /// Verifies a plain-text password against a BCrypt hash.
    /// Returns true if the password matches.
    /// </summary>
    bool Verify(string plainPassword, string hashedPassword);
}
