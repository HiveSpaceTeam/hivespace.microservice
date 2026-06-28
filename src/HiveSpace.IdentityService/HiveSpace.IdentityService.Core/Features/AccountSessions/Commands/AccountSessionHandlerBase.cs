using HiveSpace.IdentityService.Core.Features.AccountSessions.Dtos;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.Domain.Shared.Enumerations;
using Microsoft.AspNetCore.Identity;

namespace HiveSpace.IdentityService.Core.Features.AccountSessions.Commands;

internal static class AccountSessionHandlerBase
{
    public static string NormalizeApp(string app)
        => app.Trim().ToLowerInvariant();

    public static bool UserCanAccessApp(ApplicationUser user, string app, IReadOnlySet<string> roles)
    {
        var isAdmin = roles.Contains("Admin");
        var isSystemAdmin = roles.Contains("SystemAdmin");

        return NormalizeApp(app) switch
        {
            "admin" => isAdmin || isSystemAdmin,
            "seller" => !isAdmin && !isSystemAdmin,
            "buyer" => true,
            _ => false
        };
    }

    public static async Task<IReadOnlySet<string>> GetRolesAsync(UserManager<ApplicationUser> userManager, ApplicationUser user)
    {
        var roles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(user.RoleName))
            roles.Add(user.RoleName);

        foreach (var role in await userManager.GetRolesAsync(user))
            roles.Add(role);

        if (roles.Count == 0)
            roles.Add("Buyer");

        return roles;
    }

    public static async Task<bool> IsOtpSignInEligibleAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationUser? user)
    {
        if (user is null || !user.EmailConfirmed || user.Status != UserStatus.Active)
            return false;

        if (await userManager.IsLockedOutAsync(user))
            return false;

        var roles = await GetRolesAsync(userManager, user);
        return !roles.Contains("Admin") && !roles.Contains("SystemAdmin");
    }

    public static SessionUser ToSessionUser(ApplicationUser user, IReadOnlyCollection<string> roles)
    {
        return new SessionUser(
            user.Id,
            user.Email ?? string.Empty,
            user.UserName ?? user.Email,
            roles.ToArray(),
            user.EmailConfirmed,
            user.Status.ToString());
    }
}
