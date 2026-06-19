using FluentAssertions;
using HiveSpace.Core.Exceptions;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Services;
using HiveSpace.IdentityService.Core.Interfaces.Services;
using HiveSpace.IdentityService.Tests.Helpers;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Xunit;

namespace HiveSpace.IdentityService.Tests.Application.Session;

public class AccountSessionIssuerTests
{
    private static ApplicationUser ActiveBuyer() => new()
    {
        Id = Guid.NewGuid(),
        Email = "test@hivespace.local",
        EmailConfirmed = true,
        Status = UserStatus.Active,
        RoleName = "Buyer"
    };

    private static (AccountSessionIssuer issuer, UserManager<ApplicationUser> userManager, ITokenCookieService cookies, ICsrfTokenService csrf)
        Build()
    {
        var userManager = IdentityMocks.UserManager();
        var cookies = Substitute.For<ITokenCookieService>();
        var csrf = Substitute.For<ICsrfTokenService>();
        return (new AccountSessionIssuer(userManager, cookies, csrf), userManager, cookies, csrf);
    }

    [Fact]
    public async Task ValidateCanIssue_WhenUserInactive_ThrowsForbiddenException()
    {
        var user = ActiveBuyer();
        user.Status = UserStatus.Inactive;
        var (issuer, _, _, _) = Build();

        var act = () => issuer.ValidateCanIssueAsync(user, "buyer");

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task ValidateCanIssue_WhenUserLockedOut_ThrowsForbiddenException()
    {
        var user = ActiveBuyer();
        var (issuer, userManager, _, _) = Build();
        userManager.IsLockedOutAsync(user).Returns(true);

        var act = () => issuer.ValidateCanIssueAsync(user, "buyer");

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task ValidateCanIssue_WhenAppNotAllowed_ThrowsForbiddenException()
    {
        var user = ActiveBuyer(); // Buyer role cannot access "admin"
        var (issuer, userManager, _, _) = Build();
        userManager.IsLockedOutAsync(user).Returns(false);
        userManager.GetRolesAsync(user).Returns(new List<string>());

        var act = () => issuer.ValidateCanIssueAsync(user, "admin");

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task ValidateCanIssue_WithValidActiveUser_ReturnsRoles()
    {
        var user = ActiveBuyer();
        var (issuer, userManager, _, _) = Build();
        userManager.IsLockedOutAsync(user).Returns(false);
        userManager.GetRolesAsync(user).Returns(new List<string>());

        var roles = await issuer.ValidateCanIssueAsync(user, "buyer");

        roles.Should().Contain("Buyer");
    }

    [Fact]
    public async Task IssueAsync_WithUpdateLastLogin_UpdatesUserAndReturnsSession()
    {
        var user = ActiveBuyer();
        var (issuer, userManager, cookies, csrf) = Build();
        userManager.IsLockedOutAsync(user).Returns(false);
        userManager.GetRolesAsync(user).Returns(new List<string>());
        userManager.UpdateAsync(user).Returns(IdentityResult.Success);

        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        var refreshAt = DateTimeOffset.UtcNow.AddDays(7);
        cookies.IssueAsync(user, "buyer", Arg.Any<CancellationToken>())
            .Returns(new TokenCookieIssueResult("sess-1", expiresAt, refreshAt));
        csrf.Issue("sess-1", refreshAt).Returns("csrf-abc");

        var response = await issuer.IssueAsync(user, "buyer", null, updateLastLogin: true);

        await userManager.Received(1).UpdateAsync(user);
        response.CsrfToken.Should().Be("csrf-abc");
        response.ExpiresAt.Should().Be(expiresAt);
    }

    [Fact]
    public async Task IssueAsync_WithoutUpdateLastLogin_SkipsUserUpdate()
    {
        var user = ActiveBuyer();
        var (issuer, userManager, cookies, csrf) = Build();
        userManager.IsLockedOutAsync(user).Returns(false);
        userManager.GetRolesAsync(user).Returns(new List<string>());

        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        var refreshAt = DateTimeOffset.UtcNow.AddDays(7);
        cookies.IssueAsync(user, "buyer", Arg.Any<CancellationToken>())
            .Returns(new TokenCookieIssueResult("sess-2", expiresAt, refreshAt));
        csrf.Issue("sess-2", refreshAt).Returns("csrf-xyz");

        var response = await issuer.IssueAsync(user, "buyer", "/home", updateLastLogin: false);

        await userManager.DidNotReceive().UpdateAsync(Arg.Any<ApplicationUser>());
        response.RedirectTo.Should().Be("/home");
    }
}
