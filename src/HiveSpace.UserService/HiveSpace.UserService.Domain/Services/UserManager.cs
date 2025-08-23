using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.Domain.Shared.Exceptions;
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
    /// <exception cref="ConflictException">Thrown when a user with the email already exists or when a username is already taken</exception>
    public async Task<User> RegisterUserAsync(
        Email email,
        string userName,
        string passwordHash,
        string fullName,
        CancellationToken cancellationToken = default)
    {
        // Check availability (will throw specific exceptions if not available)
        await CanUserBeRegisteredAsync(email, userName?.Trim() ?? string.Empty, cancellationToken);
        
        // Create new user - validation handled in User.Create
        var user = User.Create(email, userName ?? string.Empty, passwordHash, fullName);
        
        return user;
    }
    
    /// <summary>
    /// Checks if an email is available for user registration.
    /// </summary>
    /// <param name="email">The email to check</param>
    /// <returns>True if the email is available, false otherwise</returns>
    private async Task<bool> IsEmailAvailableAsync(Email email, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userRepository.GetByEmailAsync(email, cancellationToken);
        return existingUser is null;
    }
    
    /// <summary>
    /// Checks if a username is available for user registration.
    /// </summary>
    /// <param name="userName">The username to check</param>
    /// <returns>True if the username is available, false otherwise</returns>
    private async Task<bool> IsUserNameAvailableAsync(string userName, CancellationToken cancellationToken = default)
    {
        var trimmedUserName = userName?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(trimmedUserName))
            return false;
            
        var existingUser = await _userRepository.GetByUserNameAsync(trimmedUserName, cancellationToken);
        return existingUser is null;
    }
    
    /// <summary>
    /// Validates if a user can be registered by checking email and username availability.
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="userName">User's chosen username</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the user can be registered</returns>
    /// <exception cref="ConflictException">Thrown when email is already taken or when username is already taken</exception>
    private async Task<bool> CanUserBeRegisteredAsync(Email email, string userName, CancellationToken cancellationToken = default)
    {
        // Check email availability first (more likely to be unique)
        if (!await IsEmailAvailableAsync(email, cancellationToken))
            throw new ConflictException(UserDomainErrorCode.EmailAlreadyExists, nameof(User));
            
        // Check username availability
        if (!await IsUserNameAvailableAsync(userName, cancellationToken))
            throw new ConflictException(UserDomainErrorCode.UserNameAlreadyExists, nameof(User));
            
        return true;
    }
}
