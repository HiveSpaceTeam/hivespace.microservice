using FluentAssertions;
using HiveSpace.Core.Exceptions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Dtos;
using HiveSpace.IdentityService.Core.Features.ExternalLogins.Commands.ConfirmGoogleLink;
using HiveSpace.IdentityService.Core.Interfaces.Messaging;
using HiveSpace.IdentityService.Core.Interfaces.Services;
using HiveSpace.IdentityService.Tests.Fixtures;
using HiveSpace.IdentityService.Tests.Helpers;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Xunit;

namespace HiveSpace.IdentityService.Tests.Application.GoogleAuth;

public class ConfirmGoogleLinkCommandHandlerTests : IClassFixture<IdentityServiceFixture>
{
    private readonly IdentityServiceFixture _fixture;

    public ConfirmGoogleLinkCommandHandlerTests(IdentityServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithValidPassword_LinksAccountAndReturnsSession()
    {
        var userId = Guid.NewGuid();
        var email = "buyer@hivespace.local";
        var user = new ApplicationUser
        {
            Id = userId,
            Email = email,
            RoleName = "Buyer",
            Status = UserStatus.Active,
            EmailConfirmed = true
        };
        var pending = ConfirmGoogleLinkCommandHandlerTests.MakePendingState(userId, email);
        var sessionResponse = new SessionResponse(
            new SessionUser(userId, email, null, ["Buyer"], true, "Active"),
            DateTimeOffset.UtcNow.AddHours(1), DateTimeOffset.UtcNow.AddDays(7), "csrf-1", null);

        var userManager = IdentityMocks.UserManager();
        userManager.FindByIdAsync(userId.ToString()).Returns(user);
        userManager.FindByLoginAsync("Google", "gid-1").Returns((ApplicationUser?)null);
        userManager.AddLoginAsync(Arg.Any<ApplicationUser>(), Arg.Any<UserLoginInfo>()).Returns(IdentityResult.Success);

        var signInManager = IdentityMocks.SignInManager(userManager);
        signInManager.CheckPasswordSignInAsync(user, "P@ssw0rd1", lockoutOnFailure: true)
            .Returns(SignInResult.Success);

        var store = Substitute.For<IPendingGoogleLinkStore>();
        store.GetRequiredAsync("link-token", Arg.Any<CancellationToken>()).Returns(pending);

        var issuer = Substitute.For<IAccountSessionIssuer>();
        issuer.ValidateCanIssueAsync(user, "storefront", Arg.Any<CancellationToken>())
            .Returns(new HashSet<string> { "Buyer" });
        issuer.IssueAsync(user, "storefront", null, true, Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(sessionResponse);

        var handler = new ConfirmGoogleLinkCommandHandler(
            userManager, signInManager,
            Substitute.For<IIdentityEventPublisher>(),
            _fixture.DbContext, issuer, store);

        var result = await handler.Handle(
            new ConfirmGoogleLinkCommand(true, "P@ssw0rd1", "storefront", null, null, "link-token"),
            CancellationToken.None);

        result.Should().NotBeNull();
        result.CsrfToken.Should().Be("csrf-1");
    }

    [Fact]
    public async Task Handle_WithWrongPassword_ThrowsUnauthorizedException()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, Email = "buyer@hivespace.local", Status = UserStatus.Active };
        var pending = MakePendingState(userId, "buyer@hivespace.local");

        var userManager = IdentityMocks.UserManager();
        userManager.FindByIdAsync(userId.ToString()).Returns(user);
        userManager.FindByLoginAsync("Google", "gid-1").Returns((ApplicationUser?)null);

        var signInManager = IdentityMocks.SignInManager(userManager);
        signInManager.CheckPasswordSignInAsync(user, Arg.Any<string>(), lockoutOnFailure: true)
            .Returns(SignInResult.Failed);

        var store = Substitute.For<IPendingGoogleLinkStore>();
        store.GetRequiredAsync("link-token", Arg.Any<CancellationToken>()).Returns(pending);

        var handler = new ConfirmGoogleLinkCommandHandler(
            userManager, signInManager,
            Substitute.For<IIdentityEventPublisher>(),
            _fixture.DbContext,
            Substitute.For<IAccountSessionIssuer>(), store);

        var act = () => handler.Handle(
            new ConfirmGoogleLinkCommand(true, "WrongPass", "storefront", null, null, "link-token"),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_WithAppMismatch_ThrowsBadRequestException()
    {
        var userId = Guid.NewGuid();
        var pending = MakePendingState(userId, "user@hivespace.local", "admin");

        var store = Substitute.For<IPendingGoogleLinkStore>();
        store.GetRequiredAsync("link-token", Arg.Any<CancellationToken>()).Returns(pending);

        var handler = new ConfirmGoogleLinkCommandHandler(
            IdentityMocks.UserManager(), IdentityMocks.SignInManager(IdentityMocks.UserManager()),
            Substitute.For<IIdentityEventPublisher>(),
            _fixture.DbContext,
            Substitute.For<IAccountSessionIssuer>(), store);

        var act = () => handler.Handle(
            new ConfirmGoogleLinkCommand(true, "P@ssw0rd1", "storefront", null, null, "link-token"),
            CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WithUserNotFound_ThrowsNotFoundException()
    {
        var pending = MakePendingState(Guid.NewGuid(), "ghost@hivespace.local");

        var userManager = IdentityMocks.UserManager();
        userManager.FindByIdAsync(Arg.Any<string>()).Returns((ApplicationUser?)null);

        var store = Substitute.For<IPendingGoogleLinkStore>();
        store.GetRequiredAsync("link-token", Arg.Any<CancellationToken>()).Returns(pending);

        var handler = new ConfirmGoogleLinkCommandHandler(
            userManager, IdentityMocks.SignInManager(userManager),
            Substitute.For<IIdentityEventPublisher>(),
            _fixture.DbContext,
            Substitute.For<IAccountSessionIssuer>(), store);

        var act = () => handler.Handle(
            new ConfirmGoogleLinkCommand(true, "P@ssw0rd1", "storefront", null, null, "link-token"),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithAlreadyLinkedLogin_ThrowsConflictException()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, Email = "buyer@hivespace.local", Status = UserStatus.Active };
        var pending = MakePendingState(userId, "buyer@hivespace.local");
        var existingLinked = new ApplicationUser { Id = Guid.NewGuid() };

        var userManager = IdentityMocks.UserManager();
        userManager.FindByIdAsync(userId.ToString()).Returns(user);
        userManager.FindByLoginAsync("Google", "gid-1").Returns(existingLinked);

        var store = Substitute.For<IPendingGoogleLinkStore>();
        store.GetRequiredAsync("link-token", Arg.Any<CancellationToken>()).Returns(pending);

        var handler = new ConfirmGoogleLinkCommandHandler(
            userManager, IdentityMocks.SignInManager(userManager),
            Substitute.For<IIdentityEventPublisher>(),
            _fixture.DbContext,
            Substitute.For<IAccountSessionIssuer>(), store);

        var act = () => handler.Handle(
            new ConfirmGoogleLinkCommand(true, "P@ssw0rd1", "storefront", null, null, "link-token"),
            CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenPasswordCheckLocksOut_ThrowsForbiddenException()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "locked@hivespace.local",
            EmailConfirmed = true,
            Status = UserStatus.Active
        };

        var userManager = IdentityMocks.UserManager();
        userManager.FindByIdAsync(userId.ToString()).Returns(user);
        userManager.FindByLoginAsync("Google", "gid-1").Returns((ApplicationUser?)null);

        var signInManager = IdentityMocks.SignInManager(userManager);
        signInManager.CheckPasswordSignInAsync(user, "P@ssw0rd1", true)
            .Returns(SignInResult.LockedOut);

        var store = Substitute.For<IPendingGoogleLinkStore>();
        store.GetRequiredAsync("link-token", Arg.Any<CancellationToken>())
            .Returns(MakePendingState(userId, user.Email!));

        var handler = new ConfirmGoogleLinkCommandHandler(
            userManager,
            signInManager,
            Substitute.For<IIdentityEventPublisher>(),
            _fixture.DbContext,
            Substitute.For<IAccountSessionIssuer>(),
            store);

        var act = () => handler.Handle(
            new ConfirmGoogleLinkCommand(true, "P@ssw0rd1", "storefront", null, null, "link-token"),
            CancellationToken.None);

        await act.Should().ThrowAsync<HiveSpace.Core.Exceptions.ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WhenAddLoginFails_ThrowsBadRequestException()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "add-login-fail@hivespace.local",
            EmailConfirmed = true,
            Status = UserStatus.Active
        };

        var userManager = IdentityMocks.UserManager();
        userManager.FindByIdAsync(userId.ToString()).Returns(user);
        userManager.FindByLoginAsync("Google", "gid-1").Returns((ApplicationUser?)null);
        userManager.AddLoginAsync(Arg.Any<ApplicationUser>(), Arg.Any<UserLoginInfo>())
            .Returns(IdentityResult.Failed(new IdentityError { Code = "AddLoginFailed" }));

        var signInManager = IdentityMocks.SignInManager(userManager);
        signInManager.CheckPasswordSignInAsync(user, "P@ssw0rd1", true).Returns(SignInResult.Success);

        var store = Substitute.For<IPendingGoogleLinkStore>();
        store.GetRequiredAsync("link-token", Arg.Any<CancellationToken>())
            .Returns(MakePendingState(userId, user.Email!));

        var handler = new ConfirmGoogleLinkCommandHandler(
            userManager,
            signInManager,
            Substitute.For<IIdentityEventPublisher>(),
            _fixture.DbContext,
            Substitute.For<IAccountSessionIssuer>(),
            store);

        var act = () => handler.Handle(
            new ConfirmGoogleLinkCommand(true, "P@ssw0rd1", "storefront", null, null, "link-token"),
            CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenPendingEmailMatches_UnconfirmedUserIsActivatedAndEventsPublished()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "pending-match@hivespace.local",
            FullName = "Pending Match",
            RoleName = "Buyer",
            Status = UserStatus.Pending,
            EmailConfirmed = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var publisher = Substitute.For<IIdentityEventPublisher>();
        var userManager = IdentityMocks.UserManager();
        userManager.FindByIdAsync(user.Id.ToString()).Returns(user);
        userManager.FindByLoginAsync("Google", "gid-1").Returns((ApplicationUser?)null);
        userManager.AddLoginAsync(Arg.Any<ApplicationUser>(), Arg.Any<UserLoginInfo>()).Returns(IdentityResult.Success);

        var signInManager = IdentityMocks.SignInManager(userManager);
        signInManager.CheckPasswordSignInAsync(user, "P@ssw0rd1", true).Returns(SignInResult.Success);

        var store = Substitute.For<IPendingGoogleLinkStore>();
        store.GetRequiredAsync("link-token", Arg.Any<CancellationToken>())
            .Returns(MakePendingState(user.Id, user.Email!, returnUrl: "/pending"));

        var issuer = Substitute.For<IAccountSessionIssuer>();
        issuer.ValidateCanIssueAsync(user, "storefront", Arg.Any<CancellationToken>())
            .Returns(new HashSet<string> { "Buyer" });
        issuer.IssueAsync(user, "storefront", "/pending", true, Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(new SessionResponse(
                new SessionUser(user.Id, user.Email!, user.Email, ["Buyer"], true, "Active"),
                DateTimeOffset.UtcNow.AddHours(1),
                DateTimeOffset.UtcNow.AddDays(7),
                "csrf",
                null));

        var handler = new ConfirmGoogleLinkCommandHandler(
            userManager,
            signInManager,
            publisher,
            _fixture.DbContext,
            issuer,
            store);

        await handler.Handle(
            new ConfirmGoogleLinkCommand(true, "P@ssw0rd1", "storefront", null, null, "link-token"),
            CancellationToken.None);

        var stored = await _fixture.DbContext.Users.FindAsync(user.Id);
        stored.Should().NotBeNull();
        stored!.EmailConfirmed.Should().BeTrue();
        stored.Status.Should().Be(UserStatus.Active);
        stored.ActivatedAt.Should().NotBeNull();
        await publisher.Received(1).PublishIdentityUserReadyAsync(user, user.FullName, Arg.Any<CancellationToken>());
        await publisher.Received(1).PublishEmailVerifiedAsync(user, HiveSpace.Domain.Shared.Enumerations.Culture.Vi, Arg.Any<CancellationToken>());
        await store.Received(1).ClearAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPendingEmailDoesNotMatch_DoesNotConfirmEmailOrPublishVerificationEvents()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "buyer@hivespace.local",
            RoleName = "Buyer",
            Status = UserStatus.Pending,
            EmailConfirmed = false
        };

        var userManager = IdentityMocks.UserManager();
        userManager.FindByIdAsync(user.Id.ToString()).Returns(user);
        userManager.FindByLoginAsync("Google", "gid-1").Returns((ApplicationUser?)null);
        userManager.AddLoginAsync(Arg.Any<ApplicationUser>(), Arg.Any<UserLoginInfo>()).Returns(IdentityResult.Success);

        var signInManager = IdentityMocks.SignInManager(userManager);
        signInManager.CheckPasswordSignInAsync(user, "P@ssw0rd1", true).Returns(SignInResult.Success);

        var publisher = Substitute.For<IIdentityEventPublisher>();
        var store = Substitute.For<IPendingGoogleLinkStore>();
        store.GetRequiredAsync("link-token", Arg.Any<CancellationToken>())
            .Returns(MakePendingState(user.Id, "other@hivespace.local"));

        var issuer = Substitute.For<IAccountSessionIssuer>();
        issuer.ValidateCanIssueAsync(user, "storefront", Arg.Any<CancellationToken>())
            .Returns(new HashSet<string> { "Buyer" });
        issuer.IssueAsync(user, "storefront", null, true, Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(new SessionResponse(
                new SessionUser(user.Id, user.Email!, user.Email, ["Buyer"], false, "Pending"),
                DateTimeOffset.UtcNow.AddHours(1),
                DateTimeOffset.UtcNow.AddDays(7),
                "csrf",
                null));

        var handler = new ConfirmGoogleLinkCommandHandler(
            userManager,
            signInManager,
            publisher,
            _fixture.DbContext,
            issuer,
            store);

        await handler.Handle(
            new ConfirmGoogleLinkCommand(true, "P@ssw0rd1", "storefront", null, null, "link-token"),
            CancellationToken.None);

        user.EmailConfirmed.Should().BeFalse();
        await publisher.DidNotReceive().PublishIdentityUserReadyAsync(Arg.Any<ApplicationUser>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
        await publisher.DidNotReceive().PublishEmailVerifiedAsync(Arg.Any<ApplicationUser>(), Arg.Any<HiveSpace.Domain.Shared.Enumerations.Culture>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCommandReturnUrlProvided_ItOverridesPendingReturnUrl()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "override@hivespace.local",
            EmailConfirmed = true,
            Status = UserStatus.Active
        };
        var pending = MakePendingState(userId, user.Email!, returnUrl: "/from-pending");

        var userManager = IdentityMocks.UserManager();
        userManager.FindByIdAsync(userId.ToString()).Returns(user);
        userManager.FindByLoginAsync("Google", "gid-1").Returns((ApplicationUser?)null);
        userManager.AddLoginAsync(Arg.Any<ApplicationUser>(), Arg.Any<UserLoginInfo>()).Returns(IdentityResult.Success);

        var signInManager = IdentityMocks.SignInManager(userManager);
        signInManager.CheckPasswordSignInAsync(user, "P@ssw0rd1", true).Returns(SignInResult.Success);

        var store = Substitute.For<IPendingGoogleLinkStore>();
        store.GetRequiredAsync("link-token", Arg.Any<CancellationToken>()).Returns(pending);

        var issuer = Substitute.For<IAccountSessionIssuer>();
        issuer.ValidateCanIssueAsync(user, "storefront", Arg.Any<CancellationToken>())
            .Returns(new HashSet<string> { "Buyer" });
        issuer.IssueAsync(user, "storefront", "/from-command", true, Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(new SessionResponse(
                new SessionUser(user.Id, user.Email!, user.Email, ["Buyer"], true, "Active"),
                DateTimeOffset.UtcNow.AddHours(1),
                DateTimeOffset.UtcNow.AddDays(7),
                "csrf",
                null));

        var handler = new ConfirmGoogleLinkCommandHandler(
            userManager,
            signInManager,
            Substitute.For<IIdentityEventPublisher>(),
            _fixture.DbContext,
            issuer,
            store);

        await handler.Handle(
            new ConfirmGoogleLinkCommand(true, "P@ssw0rd1", "storefront", "/from-command", null, "link-token"),
            CancellationToken.None);

        await issuer.Received(1).IssueAsync(user, "storefront", "/from-command", true, Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCommandReturnUrlMissing_UsesPendingReturnUrl()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "pending-return@hivespace.local",
            EmailConfirmed = true,
            Status = UserStatus.Active
        };
        var pending = MakePendingState(userId, user.Email!, returnUrl: "/from-pending");

        var userManager = IdentityMocks.UserManager();
        userManager.FindByIdAsync(userId.ToString()).Returns(user);
        userManager.FindByLoginAsync("Google", "gid-1").Returns((ApplicationUser?)null);
        userManager.AddLoginAsync(Arg.Any<ApplicationUser>(), Arg.Any<UserLoginInfo>()).Returns(IdentityResult.Success);

        var signInManager = IdentityMocks.SignInManager(userManager);
        signInManager.CheckPasswordSignInAsync(user, "P@ssw0rd1", true).Returns(SignInResult.Success);

        var store = Substitute.For<IPendingGoogleLinkStore>();
        store.GetRequiredAsync("link-token", Arg.Any<CancellationToken>()).Returns(pending);

        var issuer = Substitute.For<IAccountSessionIssuer>();
        issuer.ValidateCanIssueAsync(user, "storefront", Arg.Any<CancellationToken>())
            .Returns(new HashSet<string> { "Buyer" });
        issuer.IssueAsync(user, "storefront", "/from-pending", true, Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(new SessionResponse(
                new SessionUser(user.Id, user.Email!, user.Email, ["Buyer"], true, "Active"),
                DateTimeOffset.UtcNow.AddHours(1),
                DateTimeOffset.UtcNow.AddDays(7),
                "csrf",
                null));

        var handler = new ConfirmGoogleLinkCommandHandler(
            userManager,
            signInManager,
            Substitute.For<IIdentityEventPublisher>(),
            _fixture.DbContext,
            issuer,
            store);

        await handler.Handle(
            new ConfirmGoogleLinkCommand(true, "P@ssw0rd1", "storefront", null, null, "link-token"),
            CancellationToken.None);

        await issuer.Received(1).IssueAsync(user, "storefront", "/from-pending", true, Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>());
    }

    private static PendingGoogleLinkState MakePendingState(Guid targetUserId, string email, string app = "storefront", string? returnUrl = null) =>
        new("Google", "gid-1", "Google", email, targetUserId, app, returnUrl, null,
            DateTimeOffset.UtcNow.AddMinutes(10), "link-token");
}
