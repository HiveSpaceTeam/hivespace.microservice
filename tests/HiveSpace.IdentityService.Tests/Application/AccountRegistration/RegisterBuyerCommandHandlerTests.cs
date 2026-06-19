using FluentAssertions;
using HiveSpace.Core.Exceptions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Commands.RegisterAccount;
using HiveSpace.IdentityService.Core.Interfaces.Messaging;
using HiveSpace.IdentityService.Core.Interfaces.Services;
using HiveSpace.IdentityService.Tests.Fixtures;
using HiveSpace.IdentityService.Tests.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Xunit;

namespace HiveSpace.IdentityService.Tests.Application.AccountRegistration;

public class RegisterBuyerCommandHandlerTests : IClassFixture<IdentityServiceFixture>
{
    private readonly IdentityServiceFixture _fixture;

    public RegisterBuyerCommandHandlerTests(IdentityServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithNewEmail_CreatesPendingAccountAndReturnsResponse()
    {
        var userManager = IdentityMocks.UserManager();
        userManager.FindByEmailAsync(Arg.Any<string>()).Returns((ApplicationUser?)null);
        userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);
        userManager.GenerateEmailConfirmationTokenAsync(Arg.Any<ApplicationUser>()).Returns("email-token");

        var config = Substitute.For<IConfiguration>();
        config["FrontendRedirects:storefront:Origin"].Returns("https://storefront.hivespace.local");

        var handler = new RegisterAccountCommandHandler(
            userManager,
            Substitute.For<IIdentityEventPublisher>(),
            Substitute.For<IEmailVerificationResendCooldownStore>(),
            _fixture.DbContext,
            config);

        var result = await handler.Handle(
            new RegisterAccountCommand("newbuyer@hivespace.local", "P@ssw0rd1", "P@ssw0rd1", null, "storefront", null, null),
            CancellationToken.None);

        result.Should().NotBeNull();
        result.App.Should().Be("storefront");
        result.Email.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_WithExistingActiveEmail_ThrowsConflictException()
    {
        var existing = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "dup@hivespace.local",
            Status = UserStatus.Active,
            EmailConfirmed = true
        };
        var userManager = IdentityMocks.UserManager();
        userManager.FindByEmailAsync("dup@hivespace.local").Returns(existing);

        var handler = new RegisterAccountCommandHandler(
            userManager,
            Substitute.For<IIdentityEventPublisher>(),
            Substitute.For<IEmailVerificationResendCooldownStore>(),
            _fixture.DbContext,
            Substitute.For<IConfiguration>());

        var act = () => handler.Handle(
            new RegisterAccountCommand("dup@hivespace.local", "P@ssw0rd1", "P@ssw0rd1", null, "storefront", null, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WithAdminApp_ThrowsForbiddenException()
    {
        var handler = new RegisterAccountCommandHandler(
            IdentityMocks.UserManager(),
            Substitute.For<IIdentityEventPublisher>(),
            Substitute.For<IEmailVerificationResendCooldownStore>(),
            _fixture.DbContext,
            Substitute.For<IConfiguration>());

        var act = () => handler.Handle(
            new RegisterAccountCommand("admin@hivespace.local", "P@ssw0rd1", "P@ssw0rd1", null, "admin", null, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<HiveSpace.Core.Exceptions.ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WithPendingUnconfirmedUser_ThrowsPendingVerificationConflict()
    {
        var existing = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "pending@hivespace.local",
            Status = UserStatus.Pending,
            EmailConfirmed = false
        };
        var userManager = IdentityMocks.UserManager();
        userManager.FindByEmailAsync(existing.Email).Returns(existing);

        var handler = new RegisterAccountCommandHandler(
            userManager,
            Substitute.For<IIdentityEventPublisher>(),
            Substitute.For<IEmailVerificationResendCooldownStore>(),
            _fixture.DbContext,
            Substitute.For<IConfiguration>());

        var act = () => handler.Handle(
            new RegisterAccountCommand(existing.Email, "P@ssw0rd1", "P@ssw0rd1", null, "storefront", null, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenIdentityCreateFails_ThrowsBadRequestException()
    {
        var userManager = IdentityMocks.UserManager();
        userManager.FindByEmailAsync("badcreate@hivespace.local").Returns((ApplicationUser?)null);
        userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(IdentityResult.Failed(new IdentityError { Code = "CreateFailed" }));

        var config = Substitute.For<IConfiguration>();
        config["FrontendRedirects:storefront:Origin"].Returns("https://storefront.hivespace.local");

        var handler = new RegisterAccountCommandHandler(
            userManager,
            Substitute.For<IIdentityEventPublisher>(),
            Substitute.For<IEmailVerificationResendCooldownStore>(),
            _fixture.DbContext,
            config);

        var act = () => handler.Handle(
            new RegisterAccountCommand("badcreate@hivespace.local", "P@ssw0rd1", "P@ssw0rd1", null, "storefront", null, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenAppOriginMissing_FallsBackToDefaultRedirectUrl()
    {
        const string email = "fallback@hivespace.local";
        var userManager = IdentityMocks.UserManager();
        userManager.FindByEmailAsync(email).Returns((ApplicationUser?)null);
        userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);
        userManager.GenerateEmailConfirmationTokenAsync(Arg.Any<ApplicationUser>()).Returns("email-token");

        var publisher = Substitute.For<IIdentityEventPublisher>();
        var config = Substitute.For<IConfiguration>();
        config["DefaultRedirectUrl"].Returns("https://default.hivespace.local/base");

        var handler = new RegisterAccountCommandHandler(
            userManager,
            publisher,
            Substitute.For<IEmailVerificationResendCooldownStore>(),
            _fixture.DbContext,
            config);

        await handler.Handle(
            new RegisterAccountCommand(email, "P@ssw0rd1", "P@ssw0rd1", null, "seller", null, null),
            CancellationToken.None);

        await publisher.Received(1).PublishEmailVerificationRequestedAsync(
            Arg.Any<ApplicationUser>(),
            Arg.Is<string>(link => link.StartsWith("https://default.hivespace.local/base/verify-email-callback?")),
            Arg.Any<DateTime>(),
            Arg.Any<HiveSpace.Domain.Shared.Enumerations.Culture>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRedirectConfigurationMissing_ThrowsConfigurationException()
    {
        var userManager = IdentityMocks.UserManager();
        userManager.FindByEmailAsync("noconfig@hivespace.local").Returns((ApplicationUser?)null);
        userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);
        userManager.GenerateEmailConfirmationTokenAsync(Arg.Any<ApplicationUser>()).Returns("email-token");

        var handler = new RegisterAccountCommandHandler(
            userManager,
            Substitute.For<IIdentityEventPublisher>(),
            Substitute.For<IEmailVerificationResendCooldownStore>(),
            _fixture.DbContext,
            Substitute.For<IConfiguration>());

        var act = () => handler.Handle(
            new RegisterAccountCommand("noconfig@hivespace.local", "P@ssw0rd1", "P@ssw0rd1", null, "storefront", null, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<ConfigurationException>();
    }

    [Fact]
    public async Task Handle_WhenReturnUrlProvided_AppendsItToVerificationLink()
    {
        const string email = "return-url@hivespace.local";
        var userManager = IdentityMocks.UserManager();
        userManager.FindByEmailAsync(email).Returns((ApplicationUser?)null);
        userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);
        userManager.GenerateEmailConfirmationTokenAsync(Arg.Any<ApplicationUser>()).Returns("email-token");

        var publisher = Substitute.For<IIdentityEventPublisher>();
        var config = Substitute.For<IConfiguration>();
        config["FrontendRedirects:storefront:Origin"].Returns("https://storefront.hivespace.local");

        var handler = new RegisterAccountCommandHandler(
            userManager,
            publisher,
            Substitute.For<IEmailVerificationResendCooldownStore>(),
            _fixture.DbContext,
            config);

        await handler.Handle(
            new RegisterAccountCommand(email, "P@ssw0rd1", "P@ssw0rd1", null, "storefront", "/after-verify?tab=profile", null),
            CancellationToken.None);

        await publisher.Received(1).PublishEmailVerificationRequestedAsync(
            Arg.Any<ApplicationUser>(),
            Arg.Is<string>(link => link.Contains("returnUrl=%2Fafter-verify%3Ftab%3Dprofile")),
            Arg.Any<DateTime>(),
            Arg.Any<HiveSpace.Domain.Shared.Enumerations.Culture>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(null, HiveSpace.Domain.Shared.Enumerations.Culture.Vi)]
    [InlineData("en", HiveSpace.Domain.Shared.Enumerations.Culture.En)]
    [InlineData("fr", HiveSpace.Domain.Shared.Enumerations.Culture.Vi)]
    public async Task Handle_ResolvesCultureFromCommand(string? cultureCode, HiveSpace.Domain.Shared.Enumerations.Culture expectedCulture)
    {
        const string email = "culture@hivespace.local";
        var userManager = IdentityMocks.UserManager();
        userManager.FindByEmailAsync(email).Returns((ApplicationUser?)null);
        userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);
        userManager.GenerateEmailConfirmationTokenAsync(Arg.Any<ApplicationUser>()).Returns("email-token");

        var publisher = Substitute.For<IIdentityEventPublisher>();
        var config = Substitute.For<IConfiguration>();
        config["FrontendRedirects:storefront:Origin"].Returns("https://storefront.hivespace.local");

        var handler = new RegisterAccountCommandHandler(
            userManager,
            publisher,
            Substitute.For<IEmailVerificationResendCooldownStore>(),
            _fixture.DbContext,
            config);

        await handler.Handle(
            new RegisterAccountCommand(email, "P@ssw0rd1", "P@ssw0rd1", null, "storefront", null, cultureCode),
            CancellationToken.None);

        await publisher.Received(1).PublishEmailVerificationRequestedAsync(
            Arg.Any<ApplicationUser>(),
            Arg.Any<string>(),
            Arg.Any<DateTime>(),
            expectedCulture,
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("a@hivespace.local", "a@hivespace.local")]
    public async Task Handle_MasksEmailWithSingleCharLocalPart_ReturnsEmailUnchanged(string email, string expectedEmail)
    {
        var userManager = IdentityMocks.UserManager();
        userManager.FindByEmailAsync(email).Returns((ApplicationUser?)null);
        userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);
        userManager.GenerateEmailConfirmationTokenAsync(Arg.Any<ApplicationUser>()).Returns("email-token");

        var config = Substitute.For<IConfiguration>();
        config["FrontendRedirects:storefront:Origin"].Returns("https://storefront.hivespace.local");

        var handler = new RegisterAccountCommandHandler(
            userManager,
            Substitute.For<IIdentityEventPublisher>(),
            Substitute.For<IEmailVerificationResendCooldownStore>(),
            _fixture.DbContext,
            config);

        var result = await handler.Handle(
            new RegisterAccountCommand(email, "P@ssw0rd1", "P@ssw0rd1", null, "storefront", null, null),
            CancellationToken.None);

        result.Email.Should().Be(expectedEmail);
    }

    [Theory]
    [InlineData("ab@hivespace.local", "a*@hivespace.local")]
    [InlineData("alexander@hivespace.local", "a*******r@hivespace.local")]
    public async Task Handle_MasksEmailInPendingResponse(string email, string expectedMaskedEmail)
    {
        var userManager = IdentityMocks.UserManager();
        userManager.FindByEmailAsync(email).Returns((ApplicationUser?)null);
        userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);
        userManager.GenerateEmailConfirmationTokenAsync(Arg.Any<ApplicationUser>()).Returns("email-token");

        var config = Substitute.For<IConfiguration>();
        config["FrontendRedirects:storefront:Origin"].Returns("https://storefront.hivespace.local");

        var handler = new RegisterAccountCommandHandler(
            userManager,
            Substitute.For<IIdentityEventPublisher>(),
            Substitute.For<IEmailVerificationResendCooldownStore>(),
            _fixture.DbContext,
            config);

        var result = await handler.Handle(
            new RegisterAccountCommand(email, "P@ssw0rd1", "P@ssw0rd1", null, "storefront", null, null),
            CancellationToken.None);

        result.Email.Should().Be(expectedMaskedEmail);
    }

    [Fact]
    public async Task Handle_WithNonEmptyFullName_SetsFullNameOnUser()
    {
        const string email = "fullname@hivespace.local";
        var userManager = IdentityMocks.UserManager();
        userManager.FindByEmailAsync(email).Returns((ApplicationUser?)null);
        userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(IdentityResult.Success);
        userManager.GenerateEmailConfirmationTokenAsync(Arg.Any<ApplicationUser>()).Returns("email-token");

        var config = Substitute.For<IConfiguration>();
        config["FrontendRedirects:storefront:Origin"].Returns("https://storefront.hivespace.local");

        var handler = new RegisterAccountCommandHandler(
            userManager,
            Substitute.For<IIdentityEventPublisher>(),
            Substitute.For<IEmailVerificationResendCooldownStore>(),
            _fixture.DbContext,
            config);

        var result = await handler.Handle(
            new RegisterAccountCommand(email, "P@ssw0rd1", "P@ssw0rd1", "  Alice  ", "storefront", null, null),
            CancellationToken.None);

        result.Should().NotBeNull();
        await userManager.Received(1).CreateAsync(
            Arg.Is<ApplicationUser>(u => u.FullName == "Alice"),
            Arg.Any<string>());
    }
}
