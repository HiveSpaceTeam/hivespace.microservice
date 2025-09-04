using HiveSpace.UserService.Domain.Aggregates.User;
using Microsoft.AspNetCore.Identity;

namespace HiveSpace.UserService.Infrastructure.Mappers;

/// <summary>
/// Simple role mapper for converting between domain roles and Identity roles
/// </summary>
public static class RoleMapper
{
    /// <summary>
    /// Convert Role value object to role name string
    /// </summary>
    public static string ToRoleName(Role? role)
    {
        return role?.Name ?? throw new ArgumentNullException(nameof(role), "Role cannot be null. All users must have a role.");
    }

    /// <summary>
    /// Convert role name string to Role value object
    /// </summary>
    public static Role? FromRoleName(string roleName)
    {
        return roleName switch
        {
            "Seller" => Role.Seller,
            "Admin" => Role.Admin,
            "SystemAdmin" => Role.SystemAdmin,
            _ => null // Unknown role
        };
    }

    /// <summary>
    /// Get single role from a collection of role names (enforces one role only)
    /// </summary>
    public static Role? GetSingleRole(IEnumerable<string> roleNames)
    {
        var roles = roleNames.Select(FromRoleName).Where(r => r != null).ToList();
        
        return roles.Count switch
        {
            0 => null, // No role assigned
            1 => roles.First(),
            _ => throw new InvalidOperationException($"User has multiple roles: {string.Join(", ", roles.Select(r => r?.Name))}")
        };
    }
}
