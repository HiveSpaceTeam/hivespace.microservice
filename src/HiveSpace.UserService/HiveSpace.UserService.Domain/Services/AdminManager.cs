using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Domain.Aggregates.Admin;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Exceptions;
using HiveSpace.UserService.Domain.Repositories;

namespace HiveSpace.UserService.Domain.Services;

/// <summary>
/// Domain service for managing admin-related business operations with proper hierarchy validation.
/// Enforces the rule that only system admins can create system admins.
/// </summary>
public class AdminManager : IDomainService
{
    private readonly IAdminRepository _adminRepository;
    
    public AdminManager(IAdminRepository adminRepository)
    {
        _adminRepository = adminRepository ?? throw new ArgumentNullException(nameof(adminRepository));
    }

    private async Task<Admin> ValidateCreatorAdminAsync(
        Guid creatorAdminId,
        bool isSystemOperation,
        CancellationToken cancellationToken = default)
    {
        var creatorAdmin = await _adminRepository.GetByIdAsync(creatorAdminId) ?? throw new NotFoundException(UserDomainErrorCode.AdminNotFound, nameof(Admin));
        if (!creatorAdmin.IsActive)
        {
            throw new ForbiddenException(UserDomainErrorCode.AdminInactive, nameof(Admin));
        }
        
        if (isSystemOperation && !creatorAdmin.IsSystemAdmin)
        {
            throw new ForbiddenException(UserDomainErrorCode.CannotModifySystemAdmin, nameof(Admin));
        }
        
        return creatorAdmin;
    }
    
    /// <summary>
    /// Validates if the email is available for a new admin
    /// </summary>
    private async Task ValidateEmailAvailabilityAsync(
        Email email,
        CancellationToken cancellationToken = default)
    {
        var existingAdmin = await _adminRepository.GetByEmailAsync(email);
        if (existingAdmin is not null)
        {
            throw new ConflictException(UserDomainErrorCode.EmailAlreadyExists, nameof(Admin));
        }
    }

    public async Task<Admin> CreateAdminAsync(
        Email email,
        string passwordHash,
        bool isSystem,
        Guid creatorAdminId,
        CancellationToken cancellationToken = default)
    {
        await ValidateCreatorAdminAsync(creatorAdminId, isSystem, cancellationToken);
        await ValidateEmailAvailabilityAsync(email, cancellationToken);
        
        var newAdmin = Admin.Create(email, passwordHash, isSystem);
        return newAdmin;
    }
    
    /// <summary>
    /// Creates a system admin account. Only system admins can perform this operation.
    /// </summary>
    /// <param name="email">Email address for the new system admin</param>
    /// <param name="passwordHash">Hashed password for the new system admin</param>
    /// <param name="creatorAdminId">The ID of the system admin creating this account</param>
    /// <returns>The newly created system admin</returns>
    /// <exception cref="NotFoundException">Thrown when the creator admin is not found</exception>
    /// <exception cref="ForbiddenException">Thrown when the creator admin is inactive or when a non-system admin tries to create a system admin</exception>
    /// <exception cref="ConflictException">Thrown when an admin with the email already exists</exception>
    public async Task<Admin> CreateSystemAdminAsync(
        Email email,
        string passwordHash,
        Guid creatorAdminId,
        CancellationToken cancellationToken = default)
    {
        return await CreateAdminAsync(
            email,
            passwordHash,
            isSystem: true,
            creatorAdminId,
            cancellationToken);
    }
    
    /// <summary>
    /// Activates or deactivates an admin account with proper permission validation.
    /// </summary>
    /// <param name="targetAdminId">The ID of the admin to activate/deactivate</param>
    /// <param name="isActive">Whether to activate (true) or deactivate (false) the admin</param>
    /// <param name="actorAdminId">The ID of the admin performing the operation</param>
    /// <returns>The updated admin</returns>
    /// <exception cref="NotFoundException">Thrown when either admin is not found</exception>
    /// <exception cref="ForbiddenException">Thrown when the actor admin is inactive or when a non-system admin tries to modify a system admin</exception>
    public async Task<Admin> SetAdminActiveStatusAsync(
        Guid targetAdminId,
        bool isActive,
        Guid actorAdminId,
        CancellationToken cancellationToken = default)
    {        
        var actorAdmin = await _adminRepository.GetByIdAsync(actorAdminId) ?? throw new NotFoundException(UserDomainErrorCode.AdminNotFound, nameof(Admin));
        
        if (!actorAdmin.IsActive)
        {
            throw new ForbiddenException(UserDomainErrorCode.AdminInactive, nameof(Admin));
        }
        
        var targetAdmin = await _adminRepository.GetByIdAsync(targetAdminId) ?? throw new NotFoundException(UserDomainErrorCode.AdminNotFound, nameof(Admin));
        
        // System admins cannot be modified by regular admins
        if (targetAdmin.IsSystemAdmin && !actorAdmin.IsSystemAdmin)
        {
            throw new ForbiddenException(UserDomainErrorCode.CannotModifySystemAdmin, nameof(Admin));
        }
        
        // Update the active status if it's different
        if (targetAdmin.IsActive != isActive)
        {
            if (isActive)
            {
                targetAdmin.Activate();
            }
            else
            {
                targetAdmin.Deactivate();
            }
        }
        
        return targetAdmin;
    }
}