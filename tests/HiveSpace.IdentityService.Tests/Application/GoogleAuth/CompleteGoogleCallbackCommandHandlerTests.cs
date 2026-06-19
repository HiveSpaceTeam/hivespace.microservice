using System.Security.Claims;
using FluentAssertions;
using HiveSpace.Core.Exceptions;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Features.ExternalLogins.Commands.CompleteGoogleCallback;
using HiveSpace.IdentityService.Core.Features.ExternalLogins.Dtos;
using HiveSpace.IdentityService.Core.Interfaces.Messaging;
using HiveSpace.IdentityService.Core.Interfaces.Services;
using HiveSpace.IdentityService.Tests.Fixtures;
using HiveSpace.IdentityService.Tests.Helpers;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Xunit;

namespace HiveSpace.IdentityService.Tests.Application.GoogleAuth;

public class CompleteGoogleCallbackCommandHandlerTests : IClassFixture<IdentityServiceFixture>
{
    private readonly IdentityServiceFixture _fixture;

    public CompleteGoogleCallbackCommandHandlerTests(IdentityServiceFixture fixture) => _fixture = fixture;

    private static ExternalLoginInfo CreateLoginInfo(params Claim[] claims)
        => new(new ClaimsPrincipal(new ClaimsIdentity(claims)), "Google", "gid-123", "Google");

    [Fact]
    public async Task Handle_WhenNoExternalLoginInfo_ReturnsFailedOutcome()
    {
        var userManager = IdentityMocks.UserManager();
        var signInManager = IdentityMocks.SignInManager(userManager);
        signInManager.GetExternalLoginInfoAsync().Returns((ExternalLoginInfo?)null);

        var handler = new CompleteGoogleCallbackCommandHandler(
            userManager, signInManager,
            Substitute.For<IIdentityEventPublisher>(),
            _fixture.DbContext,
            Substitute.For<IAccountSessionIssuer>(),
            Substitute.For<IPendingGoogleLinkStore>());

        var result = await handler.Handle(
            new CompleteGoogleCallbackCommand("storefront", null, null),
            CancellationToken.None);

        result.Outcome.Should().Be(GoogleCallbackOutcome.Failed);
    }

    [Fact]
    public async Task Handle_WithNewGoogleUser_CreatesAccountAndReturnsSignedIn()
    {
        const string googleEmail = "google-new@hivespace.local";
        var claims = new[]
        {
            new Claim("email", googleEmail),
            new Claim("email_verified", "true"),
            new Claim(ClaimTypes.Name, "Google User")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var loginInfo = new ExternalLoginInfo(principal, "Google", "gid-123", "Google");

        var userManager = IdentityMocks.UserManager();
        var signInManager = IdentityMocks.SignInManager(userManager);
        signInManager.GetExternalLoginInfoAsync().Returns(loginInfo);
        userManager.FindByLoginAsync("Google", "gid-123").Returns((ApplicationUser?)null);
        userManager.FindByEmailAsync(googleEmail).Returns((ApplicationUser?)null);
        userManager.CreateAsync(Arg.Any<ApplicationUser>()).Returns(IdentityResult.Success);
        userManager.AddLoginAsync(Arg.Any<ApplicationUser>(), loginInfo).Returns(IdentityResult.Success);

        var handler = new CompleteGoogleCallbackCommandHandler(
            userManager, signInManager,
            Substitute.For<IIdentityEventPublisher>(),
            _fixture.DbContext,
            Substitute.For<IAccountSessionIssuer>(),
            Substitute.For<IPendingGoogleLinkStore>());

        var result = await handler.Handle(
            new CompleteGoogleCallbackCommand("storefront", null, null),
            CancellationToken.None);

        result.Outcome.Should().Be(GoogleCallbackOutcome.SignedIn);
    }

    [Fact]
    public async Task Handle_WhenGoogleEmailMissing_ReturnsFailedOutcome()
    {
        var loginInfo = CreateLoginInfo(new Claim("email_verified", "true"));
        var userManager = IdentityMocks.UserManager();
        var signInManager = IdentityMocks.SignInManager(userManager);
        signInManager.GetExternalLoginInfoAsync().Returns(loginInfo);

        var handler = new CompleteGoogleCallbackCommandHandler(
            userManager, signInManager,
            Substitute.For<IIdentityEventPublisher>(),
            _fixture.DbContext,
            Substitute.For<IAccountSessionIssuer>(),
            Substitute.For<IPendingGoogleLinkStore>());

        var result = await handler.Handle(
            new CompleteGoogleCallbackCommand(" StoreFront ", "/return", "vi"),
            CancellationToken.None);

        result.Outcome.Should().Be(GoogleCallbackOutcome.Failed);
        result.ErrorCode.Should().Be("GoogleEmailMissing");
        result.App.Should().Be("storefront");
    }

    [Fact]
    public async Task Handle_WhenGoogleEmailIsUnverified_ReturnsFailedOutcome()
    {
        var loginInfo = CreateLoginInfo(
            new Claim("email", "buyer@hivespace.local"),
            new Claim("email_verified", "false"));
        var userManager = IdentityMocks.UserManager();
        var signInManager = IdentityMocks.SignInManager(userManager);
        signInManager.GetExternalLoginInfoAsync().Returns(loginInfo);

        var handler = new CompleteGoogleCallbackCommandHandler(
            userManager, signInManager,
            Substitute.For<IIdentityEventPublisher>(),
            _fixture.DbContext,
            Substitute.For<IAccountSessionIssuer>(),
            Substitute.For<IPendingGoogleLinkStore>());

        var result = await handler.Handle(
            new CompleteGoogleCallbackCommand("storefront", null, null),
            CancellationToken.None);

        result.Outcome.Should().Be(GoogleCallbackOutcome.Failed);
        result.ErrorCode.Should().Be("GoogleEmailUnverified");
    }

    [Fact]
    public async Task Handle_WhenLoginAlreadyLinked_IssuesSessionAndReturnsSignedIn()
    {
        var linkedUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "linked@hivespace.local",
            EmailConfirmed = true,
            Status = UserStatus.Active
        };
        var loginInfo = CreateLoginInfo(
            new Claim("email", linkedUser.Email),
            new Claim("email_verified", "true"));

        var userManager = IdentityMocks.UserManager();
        userManager.FindByLoginAsync("Google", "gid-123").Returns(linkedUser);

        var signInManager = IdentityMocks.SignInManager(userManager);
        signInManager.GetExternalLoginInfoAsync().Returns(loginInfo);

        var issuer = Substitute.For<IAccountSessionIssuer>();
        issuer.IssueAsync(linkedUser, "storefront", "/after", true, Arg.Any<CancellationToken>())
            .Returns(new HiveSpace.IdentityService.Core.Features.AccountSessions.Dtos.SessionResponse(
                new HiveSpace.IdentityService.Core.Features.AccountSessions.Dtos.SessionUser(
                    linkedUser.Id, linkedUser.Email!, linkedUser.Email, ["Buyer"], true, "Active"),
                DateTimeOffset.UtcNow.AddHours(1),
                DateTimeOffset.UtcNow.AddDays(7),
                "csrf",
                null));

        var handler = new CompleteGoogleCallbackCommandHandler(
            userManager, signInManager,
            Substitute.For<IIdentityEventPublisher>(),
            _fixture.DbContext,
            issuer,
            Substitute.For<IPendingGoogleLinkStore>());

        var result = await handler.Handle(
            new CompleteGoogleCallbackCommand("storefront", "/after", null),
            CancellationToken.None);

        result.Outcome.Should().Be(GoogleCallbackOutcome.SignedIn);
        await issuer.Received(1).IssueAsync(linkedUser, "storefront", "/after", true, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenExistingUserHasNoPassword_ReturnsFailedOutcome()
    {
        const string email = "nolocal@hivespace.local";
        var existingUser = new ApplicationUser { Id = Guid.NewGuid(), Email = email };
        var loginInfo = CreateLoginInfo(new Claim("email", email), new Claim("email_verified", "1"));

        var userManager = IdentityMocks.UserManager();
        userManager.FindByLoginAsync("Google", "gid-123").Returns((ApplicationUser?)null);
        userManager.FindByEmailAsync(email).Returns(existingUser);
        userManager.HasPasswordAsync(existingUser).Returns(false);

        var signInManager = IdentityMocks.SignInManager(userManager);
        signInManager.GetExternalLoginInfoAsync().Returns(loginInfo);

        var handler = new CompleteGoogleCallbackCommandHandler(
            userManager, signInManager,
            Substitute.For<IIdentityEventPublisher>(),
            _fixture.DbContext,
            Substitute.For<IAccountSessionIssuer>(),
            Substitute.For<IPendingGoogleLinkStore>());

        var result = await handler.Handle(
            new CompleteGoogleCallbackCommand("storefront", null, null),
            CancellationToken.None);

        result.Outcome.Should().Be(GoogleCallbackOutcome.Failed);
        result.ErrorCode.Should().Be("GoogleLinkFailed");
    }

    [Fact]
    public async Task Handle_WhenExistingPasswordUserFound_ReturnsPendingLinkWithToken()
    {
        const string email = "existing@hivespace.local";
        var existingUser = new ApplicationUser { Id = Guid.NewGuid(), Email = email };
        var loginInfo = CreateLoginInfo(
            new Claim("email", $" {email} "),
            new Claim("email_verified", "true"));
        var store = Substitute.For<IPendingGoogleLinkStore>();
        store.CreateAsync(Arg.Any<PendingGoogleLinkCreateRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PendingGoogleLinkState(
                "Google",
                "gid-123",
                "Google",
                email,
                existingUser.Id,
                "buyer",
                "/pending",
                "en",
                DateTimeOffset.UtcNow.AddMinutes(10),
                "link-token"));

        var userManager = IdentityMocks.UserManager();
        userManager.FindByLoginAsync("Google", "gid-123").Returns((ApplicationUser?)null);
        userManager.FindByEmailAsync(email).Returns(existingUser);
        userManager.HasPasswordAsync(existingUser).Returns(true);

        var signInManager = IdentityMocks.SignInManager(userManager);
        signInManager.GetExternalLoginInfoAsync().Returns(loginInfo);

        var handler = new CompleteGoogleCallbackCommandHandler(
            userManager, signInManager,
            Substitute.For<IIdentityEventPublisher>(),
            _fixture.DbContext,
            Substitute.For<IAccountSessionIssuer>(),
            store);

        var result = await handler.Handle(
            new CompleteGoogleCallbackCommand("buyer", "/pending", "en"),
            CancellationToken.None);

        result.Outcome.Should().Be(GoogleCallbackOutcome.PendingLink);
        result.LinkToken.Should().Be("link-token");
        await store.Received(1).CreateAsync(
            Arg.Is<PendingGoogleLinkCreateRequest>(request =>
                request.VerifiedEmail == email
                && request.TargetAccountId == existingUser.Id
                && request.App == "buyer"
                && request.ReturnUrl == "/pending"
                && request.Culture == "en"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserCreationFails_ThrowsBadRequestException()
    {
        const string email = "create-fail@hivespace.local";
        var loginInfo = CreateLoginInfo(
            new Claim("email", email),
            new Claim("email_verified", "true"));
        var userManager = IdentityMocks.UserManager();
        userManager.FindByLoginAsync("Google", "gid-123").Returns((ApplicationUser?)null);
        userManager.FindByEmailAsync(email).Returns((ApplicationUser?)null);
        userManager.CreateAsync(Arg.Any<ApplicationUser>())
            .Returns(IdentityResult.Failed(new IdentityError { Code = "CreateFailed" }));

        var signInManager = IdentityMocks.SignInManager(userManager);
        signInManager.GetExternalLoginInfoAsync().Returns(loginInfo);

        var handler = new CompleteGoogleCallbackCommandHandler(
            userManager, signInManager,
            Substitute.For<IIdentityEventPublisher>(),
            _fixture.DbContext,
            Substitute.For<IAccountSessionIssuer>(),
            Substitute.For<IPendingGoogleLinkStore>());

        var act = () => handler.Handle(
            new CompleteGoogleCallbackCommand("storefront", null, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenAddLoginFailsAfterCreate_ThrowsBadRequestException()
    {
        const string email = "login-fail@hivespace.local";
        var loginInfo = CreateLoginInfo(
            new Claim("email", email),
            new Claim("email_verified", "true"));
        var userManager = IdentityMocks.UserManager();
        userManager.FindByLoginAsync("Google", "gid-123").Returns((ApplicationUser?)null);
        userManager.FindByEmailAsync(email).Returns((ApplicationUser?)null);
        userManager.CreateAsync(Arg.Any<ApplicationUser>()).Returns(IdentityResult.Success);
        userManager.AddLoginAsync(Arg.Any<ApplicationUser>(), Arg.Any<UserLoginInfo>())
            .Returns(IdentityResult.Failed(new IdentityError { Code = "AddLoginFailed" }));

        var signInManager = IdentityMocks.SignInManager(userManager);
        signInManager.GetExternalLoginInfoAsync().Returns(loginInfo);

        var handler = new CompleteGoogleCallbackCommandHandler(
            userManager, signInManager,
            Substitute.For<IIdentityEventPublisher>(),
            _fixture.DbContext,
            Substitute.For<IAccountSessionIssuer>(),
            Substitute.For<IPendingGoogleLinkStore>());

        var act = () => handler.Handle(
            new CompleteGoogleCallbackCommand("storefront", null, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenProviderDisplayNameIsNull_UsesFallbackDisplayName()
    {
        const string email = "null-display@hivespace.local";
        var existingUser = new ApplicationUser { Id = Guid.NewGuid(), Email = email };
        var principal = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim("email", email),
            new Claim("email_verified", "true")
        ]));
        var loginInfo = new ExternalLoginInfo(principal, "Google", "gid-null-display", null!);

        var store = Substitute.For<IPendingGoogleLinkStore>();
        store.CreateAsync(Arg.Any<PendingGoogleLinkCreateRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PendingGoogleLinkState(
                "Google", "gid-null-display", "Google", email, existingUser.Id,
                "storefront", null, null, DateTimeOffset.UtcNow.AddMinutes(10), "link-tok"));

        var userManager = IdentityMocks.UserManager();
        userManager.FindByLoginAsync("Google", "gid-null-display").Returns((ApplicationUser?)null);
        userManager.FindByEmailAsync(email).Returns(existingUser);
        userManager.HasPasswordAsync(existingUser).Returns(true);

        var signInManager = IdentityMocks.SignInManager(userManager);
        signInManager.GetExternalLoginInfoAsync().Returns(loginInfo);

        var handler = new CompleteGoogleCallbackCommandHandler(
            userManager, signInManager,
            Substitute.For<IIdentityEventPublisher>(),
            _fixture.DbContext,
            Substitute.For<IAccountSessionIssuer>(),
            store);

        var result = await handler.Handle(
            new CompleteGoogleCallbackCommand("storefront", null, null),
            CancellationToken.None);

        result.Outcome.Should().Be(GoogleCallbackOutcome.PendingLink);
        await store.Received(1).CreateAsync(
            Arg.Is<PendingGoogleLinkCreateRequest>(r => r.ProviderDisplayName == "Google"),
            Arg.Any<CancellationToken>());
    }
}
