using System.Security.Claims;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using HiveSpace.IdentityService.Core.Identity;
using Microsoft.AspNetCore.Identity;

namespace HiveSpace.IdentityService.Api.Services;

public class CustomProfileService(UserManager<ApplicationUser> userManager) : IProfileService
{
    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        var user = await userManager.GetUserAsync(context.Subject);
        if (user is null)
            return;

        var role = string.IsNullOrWhiteSpace(user.RoleName) ? "Buyer" : user.RoleName;
        var displayName = user.UserName ?? user.Email ?? string.Empty;

        var claims = new List<Claim>
        {
            new("sub", user.Id.ToString()),
            new("email", user.Email ?? string.Empty),
            new("name", displayName),
            new("username", user.UserName ?? string.Empty),
            new("email_verified", user.EmailConfirmed.ToString().ToLowerInvariant(), ClaimValueTypes.Boolean),
            new("role", role),
            new("userStatus", user.Status.ToString())
        };

        if (user.StoreId.HasValue)
            claims.Add(new Claim("store_id", user.StoreId.Value.ToString()));

        if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
            claims.Add(new Claim("phone_number", user.PhoneNumber));

        context.IssuedClaims = claims;
    }

    public async Task IsActiveAsync(IsActiveContext context)
    {
        var user = await userManager.GetUserAsync(context.Subject);
        context.IsActive = user is not null && user.Status == 1;
    }
}
