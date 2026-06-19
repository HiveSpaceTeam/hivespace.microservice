using FluentAssertions;
using HiveSpace.Core.Exceptions;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Commands.RefreshSession;
using HiveSpace.IdentityService.Core.Interfaces.Services;
using HiveSpace.IdentityService.Tests.Helpers;
using NSubstitute;
using Xunit;

namespace HiveSpace.IdentityService.Tests.Application.Session;

public class RefreshSessionCommandHandlerTests
{
    private static BrowserRefreshSession ValidSession(Guid userId, string securityStamp = "stamp-1") =>
        new("session-1", userId, "handle-1", "buyer", DateTimeOffset.UtcNow.AddHours(1), securityStamp, DateTimeOffset.UtcNow.AddHours(-1));

    [Fact]
    public async Task Handle_WithValidSession_ReturnsSessionResponse()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "refresh@hivespace.local",
            RoleName = "Buyer",
            Status = UserStatus.Active,
            EmailConfirmed = true
        };
        var session = ValidSession(userId, "stamp-abc");
        var issued = new TokenCookieIssueResult("session-new", DateTimeOffset.UtcNow.AddHours(1), DateTimeOffset.UtcNow.AddDays(7));

        var userManager = IdentityMocks.UserManager();
        userManager.FindByIdAsync(userId.ToString()).Returns(user);
        userManager.IsLockedOutAsync(user).Returns(false);
        userManager.GetSecurityStampAsync(user).Returns("stamp-abc");
        userManager.GetRolesAsync(user).Returns(new List<string>());

        var tokenCookieService = Substitute.For<ITokenCookieService>();
        tokenCookieService.GetRequiredRefreshSessionAsync(Arg.Any<CancellationToken>()).Returns(session);
        tokenCookieService.RefreshAsync(session, user, "buyer", Arg.Any<CancellationToken>()).Returns(issued);

        var csrfTokenService = Substitute.For<ICsrfTokenService>();
        csrfTokenService.Issue(issued.SessionId, issued.RefreshExpiresAt).Returns("csrf-xyz");

        var handler = new RefreshSessionCommandHandler(userManager, tokenCookieService, csrfTokenService);
        var result = await handler.Handle(new RefreshSessionCommand("buyer"), CancellationToken.None);

        result.Should().NotBeNull();
        result.CsrfToken.Should().Be("csrf-xyz");
    }

    [Fact]
    public async Task Handle_WithExpiredSession_ThrowsUnauthorizedException()
    {
        var expiredSession = new BrowserRefreshSession(
            "session-1", Guid.NewGuid(), "handle-1", "buyer",
            DateTimeOffset.UtcNow.AddHours(-1), "stamp-1", DateTimeOffset.UtcNow.AddHours(-2));

        var tokenCookieService = Substitute.For<ITokenCookieService>();
        tokenCookieService.GetRequiredRefreshSessionAsync(Arg.Any<CancellationToken>()).Returns(expiredSession);

        var handler = new RefreshSessionCommandHandler(
            IdentityMocks.UserManager(), tokenCookieService, Substitute.For<ICsrfTokenService>());

        var act = () => handler.Handle(new RefreshSessionCommand("buyer"), CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_WithUserNotFound_ThrowsUnauthorizedException()
    {
        var userId = Guid.NewGuid();
        var session = ValidSession(userId);

        var userManager = IdentityMocks.UserManager();
        userManager.FindByIdAsync(userId.ToString()).Returns((ApplicationUser?)null);

        var tokenCookieService = Substitute.For<ITokenCookieService>();
        tokenCookieService.GetRequiredRefreshSessionAsync(Arg.Any<CancellationToken>()).Returns(session);

        var handler = new RefreshSessionCommandHandler(
            userManager, tokenCookieService, Substitute.For<ICsrfTokenService>());

        var act = () => handler.Handle(new RefreshSessionCommand("buyer"), CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_WithInactiveUser_ThrowsForbiddenException()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, Status = UserStatus.Inactive };
        var session = ValidSession(userId);

        var userManager = IdentityMocks.UserManager();
        userManager.FindByIdAsync(userId.ToString()).Returns(user);

        var tokenCookieService = Substitute.For<ITokenCookieService>();
        tokenCookieService.GetRequiredRefreshSessionAsync(Arg.Any<CancellationToken>()).Returns(session);

        var handler = new RefreshSessionCommandHandler(
            userManager, tokenCookieService, Substitute.For<ICsrfTokenService>());

        var act = () => handler.Handle(new RefreshSessionCommand("buyer"), CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WithLockedUser_ThrowsForbiddenException()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, Status = UserStatus.Active };
        var session = ValidSession(userId);

        var userManager = IdentityMocks.UserManager();
        userManager.FindByIdAsync(userId.ToString()).Returns(user);
        userManager.IsLockedOutAsync(user).Returns(true);

        var tokenCookieService = Substitute.For<ITokenCookieService>();
        tokenCookieService.GetRequiredRefreshSessionAsync(Arg.Any<CancellationToken>()).Returns(session);

        var handler = new RefreshSessionCommandHandler(
            userManager, tokenCookieService, Substitute.For<ICsrfTokenService>());

        var act = () => handler.Handle(new RefreshSessionCommand("buyer"), CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WithSecurityStampMismatch_ThrowsUnauthorizedException()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, Status = UserStatus.Active };
        var session = ValidSession(userId, "stamp-old");

        var userManager = IdentityMocks.UserManager();
        userManager.FindByIdAsync(userId.ToString()).Returns(user);
        userManager.IsLockedOutAsync(user).Returns(false);
        userManager.GetSecurityStampAsync(user).Returns("stamp-new");

        var tokenCookieService = Substitute.For<ITokenCookieService>();
        tokenCookieService.GetRequiredRefreshSessionAsync(Arg.Any<CancellationToken>()).Returns(session);

        var handler = new RefreshSessionCommandHandler(
            userManager, tokenCookieService, Substitute.For<ICsrfTokenService>());

        var act = () => handler.Handle(new RefreshSessionCommand("buyer"), CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_WhenAppNotAllowedForRole_ThrowsForbiddenException()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, Status = UserStatus.Active, RoleName = "Admin" };
        var session = ValidSession(userId, "stamp-1");

        var userManager = IdentityMocks.UserManager();
        userManager.FindByIdAsync(userId.ToString()).Returns(user);
        userManager.IsLockedOutAsync(user).Returns(false);
        userManager.GetSecurityStampAsync(user).Returns("stamp-1");
        userManager.GetRolesAsync(user).Returns(new List<string>());

        var tokenCookieService = Substitute.For<ITokenCookieService>();
        tokenCookieService.GetRequiredRefreshSessionAsync(Arg.Any<CancellationToken>()).Returns(session);

        var handler = new RefreshSessionCommandHandler(
            userManager, tokenCookieService, Substitute.For<ICsrfTokenService>());

        var act = () => handler.Handle(new RefreshSessionCommand("seller"), CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
