using HiveSpace.IdentityService.Core.Features.AdminIdentity.Dtos;
using HiveSpace.IdentityService.Core.DomainModels;

namespace HiveSpace.IdentityService.Core.Features.AdminIdentity;

internal static class AdminIdentityMapper
{
    public static CreateAdminResult ToCreateAdminResult(ApplicationUser user)
        => new(user.Id, user.Email ?? string.Empty, GetDisplayName(user),
            string.Equals(user.RoleName, "SystemAdmin", StringComparison.OrdinalIgnoreCase),
            user.CreatedAt, user.UpdatedAt ?? user.CreatedAt, user.LastLoginAt ?? user.CreatedAt, user.Status == UserStatus.Active);

    public static AdminIdentityDto ToAdminDto(ApplicationUser user)
        => new(user.Id, user.UserName ?? string.Empty, GetDisplayName(user), user.Email ?? string.Empty, (int)user.Status,
            string.Equals(user.RoleName, "SystemAdmin", StringComparison.OrdinalIgnoreCase),
            user.CreatedAt, user.UpdatedAt, user.LastLoginAt, null);

    public static UserIdentityDto ToUserDto(ApplicationUser user)
        => new(user.Id, user.UserName ?? string.Empty, GetDisplayName(user), user.Email ?? string.Empty, (int)user.Status,
            string.Equals(user.RoleName, "Seller", StringComparison.OrdinalIgnoreCase),
            user.CreatedAt, user.UpdatedAt, user.LastLoginAt, null);

    public static SetIdentityStatusResult ToStatusResult(ApplicationUser user)
        => new(user.Id, user.UserName ?? string.Empty, GetDisplayName(user), user.Email ?? string.Empty, (int)user.Status,
            user.CreatedAt, user.UpdatedAt, user.LastLoginAt, null);

    public static string GetDisplayName(ApplicationUser user)
        => user.UserName ?? user.Email ?? string.Empty;
}
