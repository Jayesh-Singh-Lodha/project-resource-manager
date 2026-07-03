using PRM.Core.Interfaces;

namespace PRM.Infrastructure.Auth;

/// <summary>
/// BCrypt-based password hasher implementation.
/// Uses BCrypt.Net-Next library for secure password hashing.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    /// <inheritdoc />
    public string Hash(string plainPassword)
    {
        return BCrypt.Net.BCrypt.HashPassword(plainPassword);
    }

    /// <inheritdoc />
    public bool Verify(string plainPassword, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);
    }
}
