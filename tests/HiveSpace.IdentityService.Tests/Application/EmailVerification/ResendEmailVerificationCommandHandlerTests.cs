using FluentAssertions;
using HiveSpace.Core.Exceptions;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Features.EmailVerification.Commands.ResendEmailVerification;
using HiveSpace.IdentityService.Core.Interfaces.Messaging;
using HiveSpace.IdentityService.Core.Interfaces.Services;
using HiveSpace.IdentityService.Tests.Fixtures;
using HiveSpace.IdentityService.Tests.Helpers;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Xunit;

namespace HiveSpace.IdentityService.Tests.Application.EmailVerification;

public class ResendEmailVerificationCommandHandlerTests : IClassFixture<IdentityServiceFixture>
{
    private readonly IdentityServiceFixture _fixture;

    public ResendEmailVerificationCommandHandlerTests(IdentityServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WhenNoCooldown_ResendsSendVerificationEmail()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "resend-ok@hivespace.local",
            Status = UserStatus.Pending,
            EmailConfirmed = false
        };

        var userManager = IdentityMocks.UserManager();
        userManager.FindByEmailAsync(user.Email).Returns(user);
        userManager.GenerateEmailConfirmationTokenAsync(user).Returns("resend-token");

        var cooldownStore = Substitute.For<IEmailVerificationResendCooldownStore>();
        cooldownStore.GetCooldownEndsAtAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((DateTimeOffset?)null);

        var config = Substitute.For<IConfiguration>();
        config["FrontendRedirects:storefront:Origin"].Returns("https://storefront.hivespace.local");

        var handler = new ResendEmailVerificationCommandHandler(
            userManager,
            Substitute.For<IIdentityEventPublisher>(),
            cooldownStore,
            _fixture.DbContext,
            config);

        var act = () => handler.Handle(
            new ResendEmailVerificationCommand(user.Email, "storefront", null, null),
            CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_WhenCooldownActive_ReturnsWithoutResending()
    {
        var cooldownStore = Substitute.For<IEmailVerificationResendCooldownStore>();
        cooldownStore.GetCooldownEndsAtAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(DateTimeOffset.UtcNow.AddMinutes(5));

        var userManager = IdentityMocks.UserManager();

        var handler = new ResendEmailVerificationCommandHandler(
            userManager,
            Substitute.For<IIdentityEventPublisher>(),
            cooldownStore,
            _fixture.DbContext,
            Substitute.For<IConfiguration>());

        await handler.Handle(
            new ResendEmailVerificationCommand("cooldown@hivespace.local", "storefront", null, null),
            CancellationToken.None);

        await userManager.DidNotReceive().FindByEmailAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_WithAdminApp_ThrowsForbiddenException()
    {
        var handler = new ResendEmailVerificationCommandHandler(
            IdentityMocks.UserManager(),
            Substitute.For<IIdentityEventPublisher>(),
            Substitute.For<IEmailVerificationResendCooldownStore>(),
            _fixture.DbContext,
            Substitute.For<IConfiguration>());

        var act = () => handler.Handle(
            new ResendEmailVerificationCommand("admin@hivespace.local", "admin", null, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WithUnknownUser_ReturnsWithoutPublishing()
    {
        var userManager = IdentityMocks.UserManager();
        userManager.FindByEmailAsync("missing@hivespace.local").Returns((ApplicationUser?)null);

        var publisher = Substitute.For<IIdentityEventPublisher>();
        var cooldownStore = Substitute.For<IEmailVerificationResendCooldownStore>();
        cooldownStore.GetCooldownEndsAtAsync("MISSING@HIVESPACE.LOCAL", Arg.Any<CancellationToken>())
            .Returns((DateTimeOffset?)null);

        var handler = new ResendEmailVerificationCommandHandler(
            userManager,
            publisher,
            cooldownStore,
            _fixture.DbContext,
            Substitute.For<IConfiguration>());

        await handler.Handle(
            new ResendEmailVerificationCommand("missing@hivespace.local", "storefront", null, null),
            CancellationToken.None);

        await publisher.DidNotReceive().PublishEmailVerificationRequestedAsync(
            Arg.Any<ApplicationUser>(),
            Arg.Any<string>(),
            Arg.Any<DateTime>(),
            Arg.Any<HiveSpace.Domain.Shared.Enumerations.Culture>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithConfirmedUser_ReturnsWithoutPublishing()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "confirmed@hivespace.local",
            EmailConfirmed = true,
            Status = UserStatus.Active
        };

        var userManager = IdentityMocks.UserManager();
        userManager.FindByEmailAsync(user.Email).Returns(user);

        var publisher = Substitute.For<IIdentityEventPublisher>();
        var cooldownStore = Substitute.For<IEmailVerificationResendCooldownStore>();
        cooldownStore.GetCooldownEndsAtAsync("CONFIRMED@HIVESPACE.LOCAL", Arg.Any<CancellationToken>())
            .Returns((DateTimeOffset?)null);

        var handler = new ResendEmailVerificationCommandHandler(
            userManager,
            publisher,
            cooldownStore,
            _fixture.DbContext,
            Substitute.For<IConfiguration>());

        await handler.Handle(
            new ResendEmailVerificationCommand(user.Email, "storefront", null, null),
            CancellationToken.None);

        await publisher.DidNotReceive().PublishEmailVerificationRequestedAsync(
            Arg.Any<ApplicationUser>(),
            Arg.Any<string>(),
            Arg.Any<DateTime>(),
            Arg.Any<HiveSpace.Domain.Shared.Enumerations.Culture>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAppOriginMissing_FallsBackToDefaultRedirectUrl()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "fallback-resend@hivespace.local",
            EmailConfirmed = false,
            Status = UserStatus.Pending
        };

        var userManager = IdentityMocks.UserManager();
        userManager.FindByEmailAsync(user.Email).Returns(user);
        userManager.GenerateEmailConfirmationTokenAsync(user).Returns("resend-token");

        var publisher = Substitute.For<IIdentityEventPublisher>();
        var cooldownStore = Substitute.For<IEmailVerificationResendCooldownStore>();
        cooldownStore.GetCooldownEndsAtAsync("FALLBACK-RESEND@HIVESPACE.LOCAL", Arg.Any<CancellationToken>())
            .Returns((DateTimeOffset?)null);

        var config = Substitute.For<IConfiguration>();
        config["DefaultRedirectUrl"].Returns("https://default.hivespace.local/root");

        var handler = new ResendEmailVerificationCommandHandler(
            userManager,
            publisher,
            cooldownStore,
            _fixture.DbContext,
            config);

        await handler.Handle(
            new ResendEmailVerificationCommand(user.Email, "seller", null, null),
            CancellationToken.None);

        await publisher.Received(1).PublishEmailVerificationRequestedAsync(
            user,
            Arg.Is<string>(link => link.StartsWith("https://default.hivespace.local/root/verify-email-callback?")),
            Arg.Any<DateTime>(),
            Arg.Any<HiveSpace.Domain.Shared.Enumerations.Culture>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRedirectConfigurationMissing_ThrowsConfigurationException()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "noconfig-resend@hivespace.local",
            EmailConfirmed = false,
            Status = UserStatus.Pending
        };

        var userManager = IdentityMocks.UserManager();
        userManager.FindByEmailAsync(user.Email).Returns(user);
        userManager.GenerateEmailConfirmationTokenAsync(user).Returns("resend-token");

        var cooldownStore = Substitute.For<IEmailVerificationResendCooldownStore>();
        cooldownStore.GetCooldownEndsAtAsync("NOCONFIG-RESEND@HIVESPACE.LOCAL", Arg.Any<CancellationToken>())
            .Returns((DateTimeOffset?)null);

        var handler = new ResendEmailVerificationCommandHandler(
            userManager,
            Substitute.For<IIdentityEventPublisher>(),
            cooldownStore,
            _fixture.DbContext,
            Substitute.For<IConfiguration>());

        var act = () => handler.Handle(
            new ResendEmailVerificationCommand(user.Email, "storefront", null, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<ConfigurationException>();
    }

    [Fact]
    public async Task Handle_WhenReturnUrlProvided_AppendsItToVerificationLink()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "return-resend@hivespace.local",
            EmailConfirmed = false,
            Status = UserStatus.Pending
        };

        var userManager = IdentityMocks.UserManager();
        userManager.FindByEmailAsync(user.Email).Returns(user);
        userManager.GenerateEmailConfirmationTokenAsync(user).Returns("resend-token");

        var publisher = Substitute.For<IIdentityEventPublisher>();
        var cooldownStore = Substitute.For<IEmailVerificationResendCooldownStore>();
        cooldownStore.GetCooldownEndsAtAsync("RETURN-RESEND@HIVESPACE.LOCAL", Arg.Any<CancellationToken>())
            .Returns((DateTimeOffset?)null);

        var config = Substitute.For<IConfiguration>();
        config["FrontendRedirects:storefront:Origin"].Returns("https://storefront.hivespace.local");

        var handler = new ResendEmailVerificationCommandHandler(
            userManager,
            publisher,
            cooldownStore,
            _fixture.DbContext,
            config);

        await handler.Handle(
            new ResendEmailVerificationCommand(user.Email, "storefront", "/verify/after?step=2", null),
            CancellationToken.None);

        await publisher.Received(1).PublishEmailVerificationRequestedAsync(
            user,
            Arg.Is<string>(link => link.Contains("returnUrl=%2Fverify%2Fafter%3Fstep%3D2")),
            Arg.Any<DateTime>(),
            Arg.Any<HiveSpace.Domain.Shared.Enumerations.Culture>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(null, HiveSpace.Domain.Shared.Enumerations.Culture.Vi)]
    [InlineData("en", HiveSpace.Domain.Shared.Enumerations.Culture.En)]
    [InlineData("jp", HiveSpace.Domain.Shared.Enumerations.Culture.Vi)]
    public async Task Handle_ResolvesCultureFromCommand(string? cultureCode, HiveSpace.Domain.Shared.Enumerations.Culture expectedCulture)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = $"culture-{Guid.NewGuid()}@hivespace.local",
            EmailConfirmed = false,
            Status = UserStatus.Pending
        };

        var userManager = IdentityMocks.UserManager();
        userManager.FindByEmailAsync(user.Email).Returns(user);
        userManager.GenerateEmailConfirmationTokenAsync(user).Returns("resend-token");

        var publisher = Substitute.For<IIdentityEventPublisher>();
        var cooldownStore = Substitute.For<IEmailVerificationResendCooldownStore>();
        cooldownStore.GetCooldownEndsAtAsync(user.Email.ToUpperInvariant(), Arg.Any<CancellationToken>())
            .Returns((DateTimeOffset?)null);

        var config = Substitute.For<IConfiguration>();
        config["FrontendRedirects:storefront:Origin"].Returns("https://storefront.hivespace.local");

        var handler = new ResendEmailVerificationCommandHandler(
            userManager,
            publisher,
            cooldownStore,
            _fixture.DbContext,
            config);

        await handler.Handle(
            new ResendEmailVerificationCommand(user.Email, "storefront", null, cultureCode),
            CancellationToken.None);

        await publisher.Received(1).PublishEmailVerificationRequestedAsync(
            user,
            Arg.Any<string>(),
            Arg.Any<DateTime>(),
            expectedCulture,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSuccessful_SetsCooldownAndSavesChanges()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "savecheck@hivespace.local",
            EmailConfirmed = false,
            Status = UserStatus.Pending
        };
        var sentinel = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "sentinel-save@hivespace.local",
            UserName = "sentinel-save@hivespace.local",
            NormalizedEmail = "SENTINEL-SAVE@HIVESPACE.LOCAL",
            NormalizedUserName = "SENTINEL-SAVE@HIVESPACE.LOCAL",
            Status = UserStatus.Pending
        };
        _fixture.DbContext.Users.Add(sentinel);

        var userManager = IdentityMocks.UserManager();
        userManager.FindByEmailAsync(user.Email).Returns(user);
        userManager.GenerateEmailConfirmationTokenAsync(user).Returns("resend-token");

        var publisher = Substitute.For<IIdentityEventPublisher>();
        var cooldownStore = Substitute.For<IEmailVerificationResendCooldownStore>();
        cooldownStore.GetCooldownEndsAtAsync("SAVECHECK@HIVESPACE.LOCAL", Arg.Any<CancellationToken>())
            .Returns((DateTimeOffset?)null);

        var config = Substitute.For<IConfiguration>();
        config["FrontendRedirects:storefront:Origin"].Returns("https://storefront.hivespace.local");

        var handler = new ResendEmailVerificationCommandHandler(
            userManager,
            publisher,
            cooldownStore,
            _fixture.DbContext,
            config);

        await handler.Handle(
            new ResendEmailVerificationCommand(user.Email, "storefront", null, null),
            CancellationToken.None);

        await cooldownStore.Received(1).SetCooldownAsync(
            "SAVECHECK@HIVESPACE.LOCAL",
            Arg.Any<DateTimeOffset>(),
            Arg.Any<CancellationToken>());
        (await _fixture.DbContext.Users.FindAsync(sentinel.Id)).Should().NotBeNull();
    }
}
