using FluentAssertions;
using HiveSpace.Core.Exceptions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Features.EmailVerification.Commands.SendEmailVerification;
using HiveSpace.IdentityService.Core.Interfaces.Messaging;
using HiveSpace.IdentityService.Tests.Fixtures;
using HiveSpace.IdentityService.Tests.Helpers;
using NSubstitute;
using Xunit;

namespace HiveSpace.IdentityService.Tests.Application.EmailVerification;

public class SendEmailVerificationCommandHandlerTests : IClassFixture<IdentityServiceFixture>
{
    private readonly IdentityServiceFixture _fixture;

    public SendEmailVerificationCommandHandlerTests(IdentityServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithValidUser_PublishesVerificationEvent()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "pending@hivespace.local",
            EmailConfirmed = false,
            Status = UserStatus.Pending
        };

        var userManager = IdentityMocks.UserManager();
        userManager.FindByIdAsync(userId.ToString()).Returns(user);
        userManager.IsEmailConfirmedAsync(user).Returns(false);
        userManager.GenerateEmailConfirmationTokenAsync(user).Returns("raw-token");

        var publisher = Substitute.For<IIdentityEventPublisher>();

        var handler = new SendEmailVerificationCommandHandler(userManager, publisher, _fixture.DbContext);
        var act = () => handler.Handle(
            new SendEmailVerificationCommand(userId, "https://app.hivespace.local/verify", null),
            CancellationToken.None);

        await act.Should().NotThrowAsync();
        await publisher.Received(1).PublishEmailVerificationRequestedAsync(
            Arg.Any<ApplicationUser>(), Arg.Any<string>(), Arg.Any<DateTime>(),
            Arg.Any<HiveSpace.Domain.Shared.Enumerations.Culture>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNullCallbackUrl_ThrowsBadRequestException()
    {
        var handler = new SendEmailVerificationCommandHandler(
            IdentityMocks.UserManager(), Substitute.For<IIdentityEventPublisher>(), _fixture.DbContext);

        var act = () => handler.Handle(
            new SendEmailVerificationCommand(Guid.NewGuid(), "", null),
            CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WithUnknownUser_ThrowsNotFoundException()
    {
        var userManager = IdentityMocks.UserManager();
        userManager.FindByIdAsync(Arg.Any<string>()).Returns((ApplicationUser?)null);

        var handler = new SendEmailVerificationCommandHandler(
            userManager, Substitute.For<IIdentityEventPublisher>(), _fixture.DbContext);

        var act = () => handler.Handle(
            new SendEmailVerificationCommand(Guid.NewGuid(), "https://app.hivespace.local/verify", null),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithAlreadyConfirmedEmail_ThrowsConflictException()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, Email = "confirmed@hivespace.local", EmailConfirmed = true };

        var userManager = IdentityMocks.UserManager();
        userManager.FindByIdAsync(userId.ToString()).Returns(user);
        userManager.IsEmailConfirmedAsync(user).Returns(true);

        var handler = new SendEmailVerificationCommandHandler(
            userManager, Substitute.For<IIdentityEventPublisher>(), _fixture.DbContext);

        var act = () => handler.Handle(
            new SendEmailVerificationCommand(userId, "https://app.hivespace.local/verify", null),
            CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WithReturnUrl_AppendsItToVerificationLink()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "pending-return@hivespace.local",
            EmailConfirmed = false,
            Status = UserStatus.Pending
        };

        var userManager = IdentityMocks.UserManager();
        userManager.FindByIdAsync(userId.ToString()).Returns(user);
        userManager.IsEmailConfirmedAsync(user).Returns(false);
        userManager.GenerateEmailConfirmationTokenAsync(user).Returns("raw-token");

        var publisher = Substitute.For<IIdentityEventPublisher>();

        var handler = new SendEmailVerificationCommandHandler(userManager, publisher, _fixture.DbContext);
        await handler.Handle(
            new SendEmailVerificationCommand(userId, "https://app.hivespace.local/verify", "/after-verify"),
            CancellationToken.None);

        await publisher.Received(1).PublishEmailVerificationRequestedAsync(
            Arg.Any<ApplicationUser>(),
            Arg.Is<string>(link => link.Contains("returnUrl=%2Fafter-verify")),
            Arg.Any<DateTime>(),
            Arg.Any<HiveSpace.Domain.Shared.Enumerations.Culture>(),
            Arg.Any<CancellationToken>());
    }
}
