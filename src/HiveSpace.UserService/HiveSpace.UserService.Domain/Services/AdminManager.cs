using HiveSpace.Domain.Shared.Interfaces;
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
    
    /// <summary>
    /// Creates a new admin account with proper hierarchy validation.
    /// </summary>
    /// <param name="email">Email address for the new admin</param>
    /// <param name="passwordHash">Hashed password for the new admin</param>
    /// <param name="isSystem">Whether the new admin should be a system admin</param>
    /// <param name="creatorAdminId">The ID of the admin creating this account</param>
    /// <returns>The newly created admin</returns>
    /// <exception cref="AdminNotFoundException">Thrown when the creator admin is not found</exception>
    /// <exception cref="AdminInactiveException">Thrown when the creator admin is inactive</exception>
    /// <exception cref="CannotModifySystemAdminException">Thrown when a regular admin tries to create a system admin</exception>
    /// <exception cref="AdminAlreadyExistsException">Thrown when an admin with the email already exists</exception>
    private async Task<Admin> ValidateCreatorAdminAsync(
        Guid creatorAdminId,
        bool isSystemOperation,
        CancellationToken cancellationToken = default)
    {
        var creatorAdmin = await _adminRepository.GetByIdAsync(creatorAdminId) ?? throw new AdminNotFoundException();
        if (!creatorAdmin.IsActive)
        {
            throw new AdminInactiveException();
        }
        
        if (isSystemOperation && !creatorAdmin.IsSystemAdmin)
        {
            throw new CannotModifySystemAdminException();
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
            throw new AdminAlreadyExistsException();
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
    /// <exception cref="AdminNotFoundException">Thrown when the creator admin is not found</exception>
    /// <exception cref="AdminInactiveException">Thrown when the creator admin is inactive</exception>
    /// <exception cref="CannotModifySystemAdminException">Thrown when a non-system admin tries to create a system admin</exception>
    /// <exception cref="AdminAlreadyExistsException">Thrown when an admin with the email already exists</exception>
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
    /// <exception cref="AdminNotFoundException">Thrown when either admin is not found</exception>
    /// <exception cref="AdminInactiveException">Thrown when the actor admin is inactive</exception>
    /// <exception cref="CannotModifySystemAdminException">Thrown when a non-system admin tries to modify a system admin</exception>
    public async Task<Admin> SetAdminActiveStatusAsync(
        Guid targetAdminId,
        bool isActive,
        Guid actorAdminId,
        CancellationToken cancellationToken = default)
    {        
        var actorAdmin = await _adminRepository.GetByIdAsync(actorAdminId) ?? throw new AdminNotFoundException();
        
        if (!actorAdmin.IsActive)
        {
            throw new AdminInactiveException();
        }
        
        var targetAdmin = await _adminRepository.GetByIdAsync(targetAdminId) ?? throw new AdminNotFoundException();
        
        // System admins cannot be modified by regular admins
        if (targetAdmin.IsSystemAdmin && !actorAdmin.IsSystemAdmin)
        {
            throw new CannotModifySystemAdminException();
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