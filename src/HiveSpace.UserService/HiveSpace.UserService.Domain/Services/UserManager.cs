using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Enums;
using HiveSpace.UserService.Domain.Exceptions;
using HiveSpace.UserService.Domain.Repositories;
using static HiveSpace.UserService.Domain.Aggregates.User.Role;

namespace HiveSpace.UserService.Domain.Services;

/// <summary>
/// Domain service for managing user registration and related business operations.
/// Enforces domain rules around user creation and email uniqueness.
/// </summary>
public class UserManager : IDomainService
{
    private readonly IUserRepository _userRepository;
    // Placeholder used during creation; Infrastructure sets the real password via Identity
    private const string PasswordPlaceholder = "__TO_BE_SET_BY_IDENTITY__";

    public UserManager(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    /// <summary>
    /// Registers a new user with the provided details.
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="userName">User's chosen username</param>
    /// <param name="fullName">User's full name</param>
    /// <returns>The newly created user</returns>
    /// <exception cref="InvalidUserInformationException">Thrown when user information is invalid</exception>
    /// <exception cref="ConflictException">Thrown when a user with the email already exists or when a username is already taken</exception>
    public async Task<User> RegisterUserAsync(
        Email email,
        string userName,
        string fullName,
        CancellationToken cancellationToken = default)
    {
        // Check availability (will throw specific exceptions if not available)
        await CanUserBeRegisteredAsync(email, userName?.Trim() ?? string.Empty, cancellationToken);

        // Create new user - validation handled in User.Create
        var user = User.Create(email, userName?.Trim() ?? string.Empty, PasswordPlaceholder, fullName); return user;
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
            throw new ConflictException(UserDomainErrorCode.EmailAlreadyExists, nameof(User.Email));

        // Check username availability
        if (!await IsUserNameAvailableAsync(userName, cancellationToken))
            throw new ConflictException(UserDomainErrorCode.UserNameAlreadyExists, nameof(User.UserName));

        return true;
    }

    /// <summary>
    /// Validates if a user has admin privileges to perform administrative operations.
    /// </summary>
    /// <param name="actorUserId">The ID of the user attempting to perform the operation</param>
    /// <param name="requireSystemAdmin">Whether the operation requires system admin privileges</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The validated user with admin privileges</returns>
    /// <exception cref="NotFoundException">Thrown when the user is not found</exception>
    /// <exception cref="ForbiddenException">Thrown when the user lacks required privileges</exception>
    private async Task<User> ValidateAdminUserAsync(
        Guid actorUserId,
        bool requireSystemAdmin = false,
        CancellationToken cancellationToken = default)
    {
        var actorUser = await _userRepository.GetByIdAsync(actorUserId)
            ?? throw new NotFoundException(UserDomainErrorCode.UserNotFound, nameof(User));

        if (actorUser.Status != UserStatus.Active)
        {
            throw new ForbiddenException(UserDomainErrorCode.UserInactive, nameof(User));
        }

        if (requireSystemAdmin && !actorUser.IsSystemAdmin)
        {
            throw new ForbiddenException(UserDomainErrorCode.InsufficientPrivileges, nameof(User));
        }

        if (!requireSystemAdmin && !actorUser.IsAdmin && !actorUser.IsSystemAdmin)
        {
            throw new ForbiddenException(UserDomainErrorCode.InsufficientPrivileges, nameof(User));
        }

        return actorUser;
    }

    /// <summary>
    /// Creates a new admin user. Only existing admins can perform this operation.
    /// </summary>
    /// <param name="email">Email address for the new admin</param>
    /// <param name="userName">Username for the new admin</param>
    /// <param name="fullName">Full name of the new admin</param>
    /// <param name="role">Role to assign (Admin or SystemAdmin)</param>
    /// <param name="creatorUserId">The ID of the admin creating this account</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The newly created admin user</returns>
    /// <exception cref="NotFoundException">Thrown when the creator user is not found</exception>
    /// <exception cref="ForbiddenException">Thrown when the creator lacks privileges or when a non-system admin tries to create a system admin</exception>
    /// <exception cref="ConflictException">Thrown when a user with the email or username already exists</exception>
    public async Task<User> CreateAdminUserAsync(
        Email email,
        string userName,
        string fullName,
        Role role,
        Guid creatorUserId,
        CancellationToken cancellationToken = default)
    {
        // Validate creator has appropriate privileges
        bool requireSystemAdmin = role.Name == RoleNames.SystemAdmin;
        await ValidateAdminUserAsync(creatorUserId, requireSystemAdmin, cancellationToken);

        // Check availability (will throw specific exceptions if not available)
        await CanUserBeRegisteredAsync(email, userName?.Trim() ?? string.Empty, cancellationToken);

        // Create new admin user
        var user = User.Create(email, userName ?? string.Empty, PasswordPlaceholder, fullName, role);

        return user;
    }

    /// <summary>
    /// Creates a system admin user. Only existing system admins can perform this operation.
    /// </summary>
    /// <param name="email">Email address for the new system admin</param>
    /// <param name="userName">Username for the new system admin</param>
    /// <param name="fullName">Full name of the new system admin</param>
    /// <param name="creatorUserId">The ID of the system admin creating this account</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The newly created system admin user</returns>
    public async Task<User> CreateSystemAdminAsync(
        Email email,
        string userName,
        string fullName,
        Guid creatorUserId,
        CancellationToken cancellationToken = default)
    {
        return await CreateAdminUserAsync(
            email,
            userName,
            fullName,
            Role.SystemAdmin,
            creatorUserId,
            cancellationToken);
    }

    /// <summary>
    /// Sets the role of a user. Only admins can perform this operation, and only system admins can assign system admin roles.
    /// </summary>
    /// <param name="targetUserId">The ID of the user whose role to change</param>
    /// <param name="newRole">The new role to assign</param>
    /// <param name="actorUserId">The ID of the admin performing the operation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated user</returns>
    /// <exception cref="NotFoundException">Thrown when either user is not found</exception>
    /// <exception cref="ForbiddenException">Thrown when the actor lacks privileges or when a non-system admin tries to assign system admin role</exception>
    public async Task<User> PromoteAdminRoleAsync(
        Guid targetUserId,
        Role newRole,
        Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        // Validate actor has appropriate privileges
        bool requireSystemAdmin = newRole.Name == RoleNames.SystemAdmin;
        var actorUser = await ValidateAdminUserAsync(actorUserId, requireSystemAdmin, cancellationToken);

        var targetUser = await _userRepository.GetByIdAsync(targetUserId)
            ?? throw new NotFoundException(UserDomainErrorCode.UserNotFound, nameof(User));

        // Prevent regular admins from modifying SystemAdmin accounts
        if (targetUser.IsSystemAdmin && !actorUser.IsSystemAdmin)
            throw new ForbiddenException(UserDomainErrorCode.InsufficientPrivileges, nameof(User));

        // Set the new role
        targetUser.SetRole(newRole);

        return targetUser;
    }

    /// <summary>
    /// Activates or deactivates a user account with proper permission validation.
    /// </summary>
    /// <param name="targetUserId">The ID of the user to activate/deactivate</param>
    /// <param name="isActive">Whether to activate (true) or deactivate (false) the user</param>
    /// <param name="actorUserId">The ID of the admin performing the operation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated user</returns>
    /// <exception cref="NotFoundException">Thrown when either user is not found</exception>
    /// <exception cref="ForbiddenException">Thrown when the actor lacks privileges or when a non-system admin tries to modify a system admin</exception>
    public async Task<User> SetUserActiveStatusAsync(
        Guid targetUserId,
        bool isActive,
        Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        // Validate actor has admin privileges
        var actorUser = await ValidateAdminUserAsync(actorUserId, requireSystemAdmin: false, cancellationToken);

        var targetUser = await _userRepository.GetByIdAsync(targetUserId)
            ?? throw new NotFoundException(UserDomainErrorCode.UserNotFound, nameof(User));

        // System admins cannot be modified by regular admins
        if (targetUser.IsSystemAdmin && !actorUser.IsSystemAdmin)
        {
            throw new ForbiddenException(UserDomainErrorCode.InsufficientPrivileges, nameof(User));
        }

        // Update the active status if it's different
        if (targetUser.Status == UserStatus.Active != isActive)
        {
            if (isActive)
            {
                targetUser.Activate();
            }
            else
            {
                targetUser.Deactivate();
            }
        }

        return targetUser;
    }

    /// <summary>
    /// Validates if the current admin can delete the target user based on hierarchical permissions
    /// </summary>
    /// <param name="currentAdmin">The admin attempting the deletion</param>
    /// <param name="targetUser">The user to be deleted</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <exception cref="ForbiddenException">Thrown when deletion is not allowed</exception>
    public void ValidateUserDeletionAsync(User currentAdmin, User targetUser, CancellationToken cancellationToken = default)
    {
        // Validate admin is active
        if (currentAdmin.Status != UserStatus.Active)
            throw new ForbiddenException(UserDomainErrorCode.UserInactive, nameof(User));

        // Prevent self-deletion
        if (currentAdmin.Id == targetUser.Id)
            throw new ForbiddenException(UserDomainErrorCode.CannotDeleteOwnAccount, nameof(User));

        // Check if target user is already deleted
        if (targetUser.IsDeleted)
            throw new ConflictException(UserDomainErrorCode.UserAlreadyDeleted, nameof(User));

        // Validate hierarchical permissions
        if (targetUser.IsAdmin || targetUser.IsSystemAdmin)
        {
            // Only System Admins can delete Admin accounts
            if (!currentAdmin.IsSystemAdmin)
                throw new ForbiddenException(UserDomainErrorCode.CannotDeleteAdminAccount, nameof(User));

            // System Admins cannot delete other System Admins
            if (targetUser.IsSystemAdmin && currentAdmin.IsSystemAdmin)
                throw new ForbiddenException(UserDomainErrorCode.CannotDeleteSystemAdmin, nameof(User));
        }
    }

    /// <summary>
    /// Reconstructs a domain User from infrastructure data.
    /// This method is used when mapping from ApplicationUser to domain User.
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="userName">User's username</param>
    /// <param name="passwordHash">User's password hash</param>
    /// <param name="fullName">User's full name</param>
    /// <param name="role">User's role</param>
    /// <param name="phoneNumber">User's phone number (optional)</param>
    /// <param name="dateOfBirth">User's date of birth (optional)</param>
    /// <param name="gender">User's gender (optional)</param>
    /// <param name="storeId">User's store ID (optional)</param>
    /// <param name="status">User's status</param>
    /// <param name="createdAt">When the user was created</param>
    /// <param name="updatedAt">When the user was last updated</param>
    /// <param name="lastLoginAt">When the user last logged in</param>
    /// <returns>Reconstructed domain User</returns>
    public User ReconstructUser(
        Email email,
        string userName,
        string passwordHash,
        string fullName,
        Role role,
        PhoneNumber? phoneNumber = null,
        DateOfBirth? dateOfBirth = null,
        Gender? gender = null,
        Guid? storeId = null,
        UserStatus status = UserStatus.Active,
        DateTimeOffset? createdAt = null,
        DateTimeOffset? updatedAt = null,
        DateTimeOffset? lastLoginAt = null)
    {
        // Use the internal factory method with all optional parameters
        return User.Create(
            email: email,
            userName: userName,
            passwordHash: passwordHash,
            fullName: fullName,
            role: role,
            phoneNumber: phoneNumber,
            dateOfBirth: dateOfBirth,
            gender: gender,
            storeId: storeId,
            status: status,
            createdAt: createdAt,
            updatedAt: updatedAt,
            lastLoginAt: lastLoginAt);
    }
}
