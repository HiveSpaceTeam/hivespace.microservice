using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using HiveSpace.UserService.Infrastructure.Data;
using HiveSpace.UserService.Domain.Exceptions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.UserService.Infrastructure.Identity;

/// <summary>
/// Custom UserStore that handles role storage directly in the ApplicationUser entity
/// instead of using the traditional IdentityUserRole many-to-many relationship.
/// </summary>
public class CustomUserStore : UserStore<ApplicationUser, IdentityRole<Guid>, UserDbContext, Guid>
{
    public CustomUserStore(UserDbContext context, IdentityErrorDescriber? describer = null)
        : base(context, describer)
    {
    }

    /// <summary>
    /// Override to get roles directly from the RoleName property instead of querying IdentityUserRole table
    /// </summary>
    public override async Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        if (user == null) throw new InvalidFieldException(DomainErrorCode.ArgumentNull, nameof(user));

        // Return the role directly from the RoleName property
        var roles = new List<string>();
        if (!string.IsNullOrEmpty(user.RoleName))
        {
            roles.Add(user.RoleName);
        }

        return await Task.FromResult(roles);
    }

    /// <summary>
    /// Override to add role by setting the RoleName property directly
    /// </summary>
    public override async Task AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        if (user == null) throw new InvalidFieldException(DomainErrorCode.ArgumentNull, nameof(user));
        if (string.IsNullOrEmpty(roleName)) throw new InvalidFieldException(DomainErrorCode.ParameterRequired, nameof(roleName));

        // Validate that the role exists
        var roleEntity = await FindRoleAsync(roleName, cancellationToken);
        if (roleEntity == null)
        {
            throw new NotFoundException(UserDomainErrorCode.UserNotFound, nameof(roleName));
        }

        // Set the role directly on the user
        user.RoleName = roleName;
    }

    /// <summary>
    /// Override to remove role by clearing the RoleName property
    /// </summary>
    public override async Task RemoveFromRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        if (user == null) throw new InvalidFieldException(DomainErrorCode.ArgumentNull, nameof(user));
        if (string.IsNullOrEmpty(roleName)) throw new InvalidFieldException(DomainErrorCode.ParameterRequired, nameof(roleName));

        // Clear the role if it matches
        if (string.Equals(user.RoleName, roleName, StringComparison.OrdinalIgnoreCase))
        {
            user.RoleName = null;
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Override to check if user is in role by comparing the RoleName property
    /// </summary>
    public override async Task<bool> IsInRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        if (user == null) throw new InvalidFieldException(DomainErrorCode.ArgumentNull, nameof(user));
        if (string.IsNullOrEmpty(roleName)) throw new InvalidFieldException(DomainErrorCode.ParameterRequired, nameof(roleName));

        return await Task.FromResult(string.Equals(user.RoleName, roleName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Override to get users in role by querying the RoleName property directly
    /// </summary>
    public override async Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        if (string.IsNullOrEmpty(roleName)) throw new InvalidFieldException(DomainErrorCode.ParameterRequired, nameof(roleName));

        return await Users
            .Where(u => u.RoleName != null && u.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase))
            .ToListAsync(cancellationToken);
    }
    /// <summary>
    /// Helper method to find a role by name
    /// </summary>
    protected override async Task<IdentityRole<Guid>?> FindRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        return await Context.Roles.FirstOrDefaultAsync(r => r.Name == roleName, cancellationToken);
    }
}