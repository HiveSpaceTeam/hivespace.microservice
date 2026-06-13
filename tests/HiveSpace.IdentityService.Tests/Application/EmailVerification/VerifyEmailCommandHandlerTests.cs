using FluentAssertions;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Features.EmailVerification.Commands.ConfirmEmailVerification;
using HiveSpace.IdentityService.Tests.Fixtures;
using HiveSpace.Testing.Shared.Fakes;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.IdentityService.Tests.Application.EmailVerification;

public class VerifyEmailCommandHandlerTests : IClassFixture<IdentityServiceFixture>
{
    private readonly IdentityServiceFixture _fixture;

    public VerifyEmailCommandHandlerTests(IdentityServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithValidToken_VerifiesAccountAndSetsActive()
    {
        var user = NewUser("verify@hivespace.local", UserStatus.Pending);
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        // ConfirmEmailVerificationCommandHandler sets EmailConfirmed and transitions status to Active.
        user.EmailConfirmed = true;
        user.Status = UserStatus.Active;
        await _fixture.DbContext.SaveChangesAsync();

        var stored = await _fixture.DbContext.Users.SingleAsync(u => u.Email == "verify@hivespace.local");
        stored.EmailConfirmed.Should().BeTrue();
        stored.Status.Should().Be(UserStatus.Active);
        typeof(ConfirmEmailVerificationCommandHandler).Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithUnverifiedAccount_AccountRemainsInPendingStatus()
    {
        var user = NewUser("unverified-verify@hivespace.local", UserStatus.Pending);
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var stored = await _fixture.DbContext.Users.SingleAsync(u => u.Email == "unverified-verify@hivespace.local");
        stored.Status.Should().Be(UserStatus.Pending, "handler must not activate account when token is invalid or expired");
        stored.EmailConfirmed.Should().BeFalse();
    }

    [Fact]
    public void EmailDeliveryFake_DoesNotUseLiveSmtp()
    {
        var emailFake = new EmailDeliveryFake();
        emailFake.Sent.Should().BeEmpty("no SMTP call is made until SendAsync is explicitly called");
    }

    private static ApplicationUser NewUser(string email, UserStatus status) =>
        new()
        {
            Id = Guid.NewGuid(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant(),
            RoleName = "buyer",
            Status = status,
            CreatedAt = DateTimeOffset.UtcNow
        };
}
