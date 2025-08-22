using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Exceptions;
using HiveSpace.UserService.Domain.Repositories;

namespace HiveSpace.UserService.Domain.Services;

/// <summary>
/// Domain service for managing user registration and related business operations.
/// Enforces domain rules around user creation and email uniqueness.
/// </summary>
public class UserManager : IDomainService
{
    private readonly IUserRepository _userRepository;
    
    public UserManager(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }
    
    /// <summary>
    /// Registers a new user with the provided details.
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="userName">User's chosen username</param>
    /// <param name="passwordHash">Hashed password</param>
    /// <param name="fullName">User's full name</param>
    /// <returns>The newly created user</returns>
    /// <exception cref="InvalidUserInformationException">Thrown when user information is invalid</exception>
    /// <exception cref="InvalidPasswordHashException">Thrown when password hash is invalid</exception>
    /// <exception cref="UserAlreadyExistsException">Thrown when a user with the email already exists</exception>
    /// <exception cref="UserNameAlreadyExistsException">Thrown when a username is already taken</exception>
    public async Task<User> RegisterUserAsync(
        Email email,
        string userName,
        string passwordHash,
        string fullName,
        CancellationToken cancellationToken = default)
    {
        // Validate input parameters using null-coalescing
        if (!ValidateUserInformation(email, userName?.Trim() ?? string.Empty, fullName?.Trim() ?? string.Empty))
            throw new InvalidUserInformationException();
            
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new InvalidPasswordHashException();
        
        // Check availability (will throw specific exceptions if not available)
        await CanUserBeRegisteredAsync(email, userName?.Trim() ?? string.Empty);
        
        // Create new user with trimmed inputs
        var trimmedUserName = userName?.Trim() ?? string.Empty;
        var trimmedFullName = fullName?.Trim() ?? string.Empty;
        var user = User.Create(email, trimmedUserName, passwordHash, trimmedFullName);
        
        return user;
    }
    
    /// <summary>
    /// Checks if an email is available for user registration.
    /// </summary>
    /// <param name="email">The email to check</param>
    /// <returns>True if the email is available, false otherwise</returns>
    private async Task<bool> IsEmailAvailableAsync(Email email)
    {
        var existingUser = await _userRepository.GetByEmailAsync(email);
        return existingUser is null;
    }
    
    /// <summary>
    /// Checks if a username is available for user registration.
    /// </summary>
    /// <param name="userName">The username to check</param>
    /// <returns>True if the username is available, false otherwise</returns>
    private async Task<bool> IsUserNameAvailableAsync(string userName)
    {
        var trimmedUserName = userName?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(trimmedUserName))
            return false;
            
        var existingUser = await _userRepository.GetByUserNameAsync(trimmedUserName);
        return existingUser is null;
    }
    
    /// <summary>
    /// Validates user information before registration.
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="userName">User's chosen username</param>
    /// <param name="fullName">User's full name</param>
    /// <returns>True if all information is valid</returns>
    private bool ValidateUserInformation(Email email, string userName, string fullName)
    {
        // Username validation with null-coalescing
        var trimmedUserName = userName?.Trim() ?? string.Empty;
        if (trimmedUserName.Length < 3 || trimmedUserName.Length > 50)
            return false;
            
        // Username should not contain invalid characters
        if (ContainsInvalidUsernameCharacters(trimmedUserName))
            return false;
        
        // Full name validation with null-coalescing
        var trimmedFullName = fullName?.Trim() ?? string.Empty;
        if (trimmedFullName.Length < 2 || trimmedFullName.Length > 100)
            return false;
        
        // Email is already validated by the Email value object
        return true;
    }
    
    /// <summary>
    /// Validates if a user can be registered by checking email and username availability.
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="userName">User's chosen username</param>
    /// <returns>True if the user can be registered</returns>
    /// <exception cref="UserAlreadyExistsException">Thrown when email is already taken</exception>
    /// <exception cref="ArgumentException">Thrown when username is already taken</exception>
    private async Task<bool> CanUserBeRegisteredAsync(Email email, string userName)
    {
        // Check email availability first (more likely to be unique)
        if (!await IsEmailAvailableAsync(email))
            throw new UserAlreadyExistsException();
            
        // Check username availability
        if (!await IsUserNameAvailableAsync(userName))
            throw new UserNameAlreadyExistsException();
            
        return true;
    }
    
    /// <summary>
    /// Checks if the username contains invalid characters.
    /// </summary>
    /// <param name="userName">Username to validate</param>
    /// <returns>True if contains invalid characters, false otherwise</returns>
    private static bool ContainsInvalidUsernameCharacters(string userName)
    {
        // Allow only alphanumeric characters, underscore, and hyphen
        return !userName.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '-');
    }
}
