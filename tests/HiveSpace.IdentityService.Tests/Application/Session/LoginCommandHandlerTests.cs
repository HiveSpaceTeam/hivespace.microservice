using FluentAssertions;
using HiveSpace.Core.Exceptions;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Commands.SignIn;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Dtos;
using HiveSpace.IdentityService.Core.Interfaces.Services;
using HiveSpace.IdentityService.Tests.Helpers;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Xunit;

namespace HiveSpace.IdentityService.Tests.Application.Session;

public class LoginCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCredentials_ReturnsSessionResponse()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "login@hivespace.local",
            EmailConfirmed = true,
            Status = UserStatus.Active,
            RoleName = "Buyer"
        };

        var userManager = IdentityMocks.UserManager();
        userManager.FindByEmailAsync("login@hivespace.local").Returns(user);

        var signInManager = IdentityMocks.SignInManager(userManager);
        signInManager.CheckPasswordSignInAsync(user, "P@ssw0rd1", lockoutOnFailure: true)
            .Returns(SignInResult.Success);

        var sessionResponse = new SessionResponse(
            new SessionUser(user.Id, user.Email!, null, ["Buyer"], true, "Active"),
            DateTimeOffset.UtcNow.AddHours(1),
            DateTimeOffset.UtcNow.AddDays(7),
            "csrf-token",
            null);

        var issuer = Substitute.For<IAccountSessionIssuer>();
        issuer.ValidateCanIssueAsync(user, "storefront", Arg.Any<CancellationToken>())
            .Returns(new HashSet<string> { "Buyer" });
        issuer.IssueAsync(user, "storefront", null, true, Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(sessionResponse);

        var handler = new SignInCommandHandler(userManager, signInManager, issuer);
        var result = await handler.Handle(
            new SignInCommand("login@hivespace.local", "P@ssw0rd1", "storefront", null, null),
            CancellationToken.None);

        result.Should().NotBeNull();
        result.CsrfToken.Should().Be("csrf-token");
    }

    [Fact]
    public async Task Handle_WithUnknownEmail_ThrowsUnauthorizedException()
    {
        var userManager = IdentityMocks.UserManager();
        userManager.FindByEmailAsync(Arg.Any<string>()).Returns((ApplicationUser?)null);

        var handler = new SignInCommandHandler(
            userManager,
            IdentityMocks.SignInManager(userManager),
            Substitute.For<IAccountSessionIssuer>());

        var act = () => handler.Handle(
            new SignInCommand("nobody@hivespace.local", "P@ssw0rd1", "storefront", null, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_WithWrongPassword_ThrowsUnauthorizedException()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "wrongpwd@hivespace.local",
            EmailConfirmed = true,
            Status = UserStatus.Active
        };

        var userManager = IdentityMocks.UserManager();
        userManager.FindByEmailAsync("wrongpwd@hivespace.local").Returns(user);

        var signInManager = IdentityMocks.SignInManager(userManager);
        signInManager.CheckPasswordSignInAsync(user, Arg.Any<string>(), lockoutOnFailure: true)
            .Returns(SignInResult.Failed);

        var issuer = Substitute.For<IAccountSessionIssuer>();
        issuer.ValidateCanIssueAsync(user, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new HashSet<string> { "Buyer" });

        var handler = new SignInCommandHandler(userManager, signInManager, issuer);
        var act = () => handler.Handle(
            new SignInCommand("wrongpwd@hivespace.local", "WrongPass", "storefront", null, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_WithUnconfirmedEmail_ThrowsForbiddenException()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "unconfirmed@hivespace.local",
            EmailConfirmed = false,
            Status = UserStatus.Pending
        };

        var userManager = IdentityMocks.UserManager();
        userManager.FindByEmailAsync(user.Email).Returns(user);

        var handler = new SignInCommandHandler(
            userManager,
            IdentityMocks.SignInManager(userManager),
            Substitute.For<IAccountSessionIssuer>());

        var act = () => handler.Handle(
            new SignInCommand(user.Email, "P@ssw0rd1", "storefront", null, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WithLockedOutPasswordResult_ThrowsForbiddenException()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "lockedout@hivespace.local",
            EmailConfirmed = true,
            Status = UserStatus.Active
        };

        var userManager = IdentityMocks.UserManager();
        userManager.FindByEmailAsync(user.Email).Returns(user);

        var signInManager = IdentityMocks.SignInManager(userManager);
        signInManager.CheckPasswordSignInAsync(user, "P@ssw0rd1", true).Returns(SignInResult.LockedOut);

        var issuer = Substitute.For<IAccountSessionIssuer>();
        issuer.ValidateCanIssueAsync(user, "storefront", Arg.Any<CancellationToken>())
            .Returns(new HashSet<string> { "Buyer" });

        var handler = new SignInCommandHandler(userManager, signInManager, issuer);

        var act = () => handler.Handle(
            new SignInCommand(user.Email, "P@ssw0rd1", "storefront", null, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WhenValidateCanIssueFails_PropagatesException()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "blocked@hivespace.local",
            EmailConfirmed = true,
            Status = UserStatus.Active
        };

        var userManager = IdentityMocks.UserManager();
        userManager.FindByEmailAsync(user.Email).Returns(user);

        var issuer = Substitute.For<IAccountSessionIssuer>();
        var expected = new ForbiddenException([]);
        issuer.ValidateCanIssueAsync(user, "admin", Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlySet<string>>>(_ => throw expected);

        var signInManager = IdentityMocks.SignInManager(userManager);
        var handler = new SignInCommandHandler(userManager, signInManager, issuer);

        var act = () => handler.Handle(
            new SignInCommand(user.Email, "P@ssw0rd1", "admin", null, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>()
            .Where(ex => ReferenceEquals(ex, expected));
        await signInManager.DidNotReceive().CheckPasswordSignInAsync(
            Arg.Any<ApplicationUser>(),
            Arg.Any<string>(),
            Arg.Any<bool>());
    }
}
