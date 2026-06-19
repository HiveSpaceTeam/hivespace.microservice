using System.Text;
using FluentAssertions;
using HiveSpace.Core.Exceptions;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Features.EmailVerification.Commands.ConfirmEmailVerification;
using HiveSpace.IdentityService.Core.Interfaces.Messaging;
using HiveSpace.IdentityService.Tests.Fixtures;
using HiveSpace.IdentityService.Tests.Helpers;
using HiveSpace.Testing.Shared.Fakes;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using NSubstitute;
using Xunit;

namespace HiveSpace.IdentityService.Tests.Application.EmailVerification;

public class VerifyEmailCommandHandlerTests : IClassFixture<IdentityServiceFixture>
{
    private readonly IdentityServiceFixture _fixture;

    public VerifyEmailCommandHandlerTests(IdentityServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithValidToken_ConfirmsEmailAndActivatesAccount()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "verify-real@hivespace.local",
            Status = UserStatus.Pending,
            EmailConfirmed = false
        };
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes("test-token"));

        var userManager = IdentityMocks.UserManager();
        userManager.FindByIdAsync(user.Id.ToString()).Returns(user);
        userManager.IsEmailConfirmedAsync(user).Returns(false);
        userManager.ConfirmEmailAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);

        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new ConfirmEmailVerificationCommandHandler(
            userManager,
            Substitute.For<IIdentityEventPublisher>(),
            _fixture.DbContext);

        var act = () => handler.Handle(
            new ConfirmEmailVerificationCommand(user.Id.ToString(), encodedToken),
            CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_WithInvalidToken_ThrowsBadRequestException()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "verify-bad@hivespace.local",
            Status = UserStatus.Pending,
            EmailConfirmed = false
        };
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes("bad-token"));

        var userManager = IdentityMocks.UserManager();
        userManager.FindByIdAsync(user.Id.ToString()).Returns(user);
        userManager.IsEmailConfirmedAsync(user).Returns(false);
        userManager.ConfirmEmailAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(IdentityResult.Failed(new IdentityError { Code = "InvalidToken", Description = "Invalid token." }));

        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new ConfirmEmailVerificationCommandHandler(
            userManager,
            Substitute.For<IIdentityEventPublisher>(),
            _fixture.DbContext);

        var act = () => handler.Handle(
            new ConfirmEmailVerificationCommand(user.Id.ToString(), encodedToken),
            CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public void EmailDeliveryFake_DoesNotUseLiveSmtp()
    {
        var emailFake = new EmailDeliveryFake();
        emailFake.Sent.Should().BeEmpty("no SMTP call is made until SendAsync is explicitly called");
    }

    [Fact]
    public async Task Handle_WithUnknownUser_ThrowsNotFoundException()
    {
        var userManager = IdentityMocks.UserManager();
        userManager.FindByIdAsync(Arg.Any<string>()).Returns((ApplicationUser?)null);

        var handler = new ConfirmEmailVerificationCommandHandler(
            userManager,
            Substitute.For<IIdentityEventPublisher>(),
            _fixture.DbContext);

        var act = () => handler.Handle(
            new ConfirmEmailVerificationCommand(Guid.NewGuid().ToString(), "token"),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithInvalidBase64Token_ThrowsBadRequestException()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "verify-invalid-base64@hivespace.local",
            Status = UserStatus.Pending,
            EmailConfirmed = false
        };

        var userManager = IdentityMocks.UserManager();
        userManager.FindByIdAsync(user.Id.ToString()).Returns(user);
        userManager.IsEmailConfirmedAsync(user).Returns(false);

        var handler = new ConfirmEmailVerificationCommandHandler(
            userManager,
            Substitute.For<IIdentityEventPublisher>(),
            _fixture.DbContext);

        var act = () => handler.Handle(
            new ConfirmEmailVerificationCommand(user.Id.ToString(), "%%%not-base64%%%"),
            CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyConfirmedAndUserAlreadyReady_ReturnsWithoutPublishing()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "already-ready@hivespace.local",
            Status = UserStatus.Active,
            ActivatedAt = DateTimeOffset.UtcNow,
            EmailConfirmed = true
        };

        var userManager = IdentityMocks.UserManager();
        userManager.FindByIdAsync(user.Id.ToString()).Returns(user);
        userManager.IsEmailConfirmedAsync(user).Returns(true);

        var publisher = Substitute.For<IIdentityEventPublisher>();
        var handler = new ConfirmEmailVerificationCommandHandler(userManager, publisher, _fixture.DbContext);

        await handler.Handle(
            new ConfirmEmailVerificationCommand(user.Id.ToString(), "ignored"),
            CancellationToken.None);

        await publisher.DidNotReceive().PublishIdentityUserReadyAsync(Arg.Any<ApplicationUser>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
        await publisher.DidNotReceive().PublishEmailVerifiedAsync(Arg.Any<ApplicationUser>(), Arg.Any<Culture>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyConfirmedButUserNotReady_PublishesReadinessEvents()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "owed-readiness@hivespace.local",
            FullName = "Owed Readiness",
            Status = UserStatus.Pending,
            ActivatedAt = null,
            EmailConfirmed = true
        };
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var userManager = IdentityMocks.UserManager();
        userManager.FindByIdAsync(user.Id.ToString()).Returns(user);
        userManager.IsEmailConfirmedAsync(user).Returns(true);

        var publisher = Substitute.For<IIdentityEventPublisher>();
        var handler = new ConfirmEmailVerificationCommandHandler(userManager, publisher, _fixture.DbContext);

        await handler.Handle(
            new ConfirmEmailVerificationCommand(user.Id.ToString(), "ignored"),
            CancellationToken.None);

        var stored = await _fixture.DbContext.Users.FindAsync(user.Id);
        stored!.Status.Should().Be(UserStatus.Active);
        stored.ActivatedAt.Should().NotBeNull();
        await publisher.Received(1).PublishIdentityUserReadyAsync(user, user.FullName, Arg.Any<CancellationToken>());
        await publisher.Received(1).PublishEmailVerifiedAsync(user, Culture.Vi, Arg.Any<CancellationToken>());
    }
}
