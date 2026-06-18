using HiveSpace.IdentityService.Core.DomainModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace HiveSpace.IdentityService.Tests.Helpers;

internal static class IdentityMocks
{
    internal static UserManager<ApplicationUser> UserManager()
    {
        var store = Substitute.For<IUserStore<ApplicationUser>>();
        return Substitute.For<UserManager<ApplicationUser>>(
            store, null, null, null, null, null, null, null, null);
    }

    internal static SignInManager<ApplicationUser> SignInManager(UserManager<ApplicationUser> userManager)
        => Substitute.For<SignInManager<ApplicationUser>>(
            userManager,
            Substitute.For<IHttpContextAccessor>(),
            Substitute.For<IUserClaimsPrincipalFactory<ApplicationUser>>(),
            null,
            null,
            Substitute.For<IAuthenticationSchemeProvider>(),
            null);
}
