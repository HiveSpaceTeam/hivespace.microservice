using HiveSpace.Infrastructure.Persistence.Repositories;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Enums;
using HiveSpace.UserService.Domain.Services;
using HiveSpace.UserService.Infrastructure.Data;
using HiveSpace.UserService.Infrastructure.Identity;
using HiveSpace.UserService.Infrastructure.Mappers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.UserService.Infrastructure.Repositories;

/// <summary>
/// Repository for User aggregate with complete aggregate loading and persistence
/// Use this repository when you need complete User aggregates with all related entities
/// </summary>
public class UserAggregateRepository : BaseRepository<User, Guid>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly UserDbContext _userDbContext;
    private readonly Domain.Services.UserManager _domainUserManager;

    public UserAggregateRepository(
        UserDbContext context, 
        UserManager<ApplicationUser> userManager,
        Domain.Services.UserManager domainUserManager) : base(context)
    {
        _userManager = userManager;
        _userDbContext = context;
        _domainUserManager = domainUserManager;
    }

    /// <summary>
    /// Gets complete User aggregate by ID including all related entities (addresses, role)
    /// This is the main method for loading complete aggregates in DDD
    /// </summary>
    public async Task<User?> GetCompleteAggregateByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Load ApplicationUser with all related data
        var applicationUser = await _userDbContext.Users
            .Include(u => u.Addresses) // Load addresses
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (applicationUser == null)
            return null;

        // Load user roles
        var roleNames = await _userManager.GetRolesAsync(applicationUser);
        
        // Convert to complete domain aggregate using UserMapper
        return applicationUser.ToDomainUser(roleNames, _domainUserManager);
    }

    /// <summary>
    /// Gets complete User aggregate by email
    /// </summary>
    public async Task<User?> GetCompleteAggregateByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        var applicationUser = await _userDbContext.Users
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.Email == email.Value, cancellationToken);

        if (applicationUser == null)
            return null;

        var roleNames = await _userManager.GetRolesAsync(applicationUser);
        return applicationUser.ToDomainUser(roleNames, _domainUserManager);
    }

    /// <summary>
    /// Updates complete User aggregate including role changes and related entities
    /// This handles the full aggregate persistence in DDD
    /// </summary>
    public async Task<User> UpdateCompleteAggregateAsync(User domainUser, CancellationToken cancellationToken = default)
    {
        // Load the tracked ApplicationUser
        var trackedApplicationUser = await _userDbContext.Users
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.Id == domainUser.Id, cancellationToken);

        if (trackedApplicationUser == null)
            throw new InvalidOperationException($"User with ID {domainUser.Id} not found for update");

        // Update ApplicationUser properties using mapper
        UserMapper.UpdateApplicationUser(trackedApplicationUser, domainUser);

        // Update role if changed
        await UpdateUserRoleAsync(trackedApplicationUser, domainUser.Role);

        // Save all changes
        await SaveChangesAsync(cancellationToken);

        // Return updated domain aggregate
        var updatedRoleNames = await _userManager.GetRolesAsync(trackedApplicationUser);
        return trackedApplicationUser.ToDomainUser(updatedRoleNames, _domainUserManager);
    }

    /// <summary>
    /// Creates a new user with complete aggregate data
    /// </summary>
    public async Task<User> CreateUserAggregateAsync(User domainUser, string password, CancellationToken cancellationToken = default)
    {
        // Convert to ApplicationUser
        var applicationUser = domainUser.ToApplicationUser();

        // Create user with Identity
        var result = await _userManager.CreateAsync(applicationUser, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create user: {errors}");
        }

        // Assign role if specified
        if (domainUser.Role != null)
        {
            var roleName = RoleMapper.ToRoleName(domainUser.Role);
            await _userManager.AddToRoleAsync(applicationUser, roleName);
        }

        // Add addresses if any
        if (domainUser.Addresses.Any())
        {
            applicationUser.Addresses = domainUser.Addresses.ToList();
            await _userDbContext.SaveChangesAsync(cancellationToken);
        }

        // Return complete domain aggregate
        var roleNames = await _userManager.GetRolesAsync(applicationUser);
        return applicationUser.ToDomainUser(roleNames, _domainUserManager);
    }

    /// <summary>
    /// Updates user role ensuring single role constraint
    /// </summary>
    private async Task UpdateUserRoleAsync(ApplicationUser applicationUser, Role? newRole)
    {
        var currentRoles = await _userManager.GetRolesAsync(applicationUser);
        var currentRole = RoleMapper.GetSingleRole(currentRoles);

        // If role changed
        if (currentRole?.Name != newRole?.Name)
        {
            // Remove all current roles (enforce single role)
            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(applicationUser, currentRoles);
            }

            // Add new role if specified
            if (newRole != null)
            {
                var roleName = RoleMapper.ToRoleName(newRole);
                await _userManager.AddToRoleAsync(applicationUser, roleName);
            }
        }
    }
}
