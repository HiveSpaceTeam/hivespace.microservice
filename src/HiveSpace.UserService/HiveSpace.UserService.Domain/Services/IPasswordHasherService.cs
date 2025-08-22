namespace HiveSpace.UserService.Domain.Services;

/// <summary>
/// Interface for password hashing operations used in the domain layer.
/// Implementation should be provided by the Infrastructure layer.
/// </summary>
public interface IPasswordHasherService
{
    /// <summary>
    /// Hashes a plaintext password using a secure hashing algorithm.
    /// </summary>
    /// <param name="password">The plaintext password to hash</param>
    /// <returns>The hashed password</returns>
    string HashPassword(string password);
    
    /// <summary>
    /// Verifies that a plaintext password matches a hashed password.
    /// </summary>
    /// <param name="hashedPassword">The hashed password to verify against</param>
    /// <param name="providedPassword">The plaintext password to verify</param>
    /// <returns>True if the password matches, false otherwise</returns>
    bool VerifyHashedPassword(string hashedPassword, string providedPassword);
}
