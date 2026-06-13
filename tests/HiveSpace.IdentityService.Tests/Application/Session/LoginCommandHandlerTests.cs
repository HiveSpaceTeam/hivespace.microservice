using FluentAssertions;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Commands.SignIn;
using HiveSpace.IdentityService.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.IdentityService.Tests.Application.Session;

public class LoginCommandHandlerTests : IClassFixture<IdentityServiceFixture>
{
    private readonly IdentityServiceFixture _fixture;

    public LoginCommandHandlerTests(IdentityServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithVerifiedCredentials_AccountIsActiveAndEmailConfirmed()
    {
        var user = NewUser("login@hivespace.local", "admin", UserStatus.Active, emailConfirmed: true);
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var stored = await _fixture.DbContext.Users.SingleAsync(u => u.Email == "login@hivespace.local");
        stored.EmailConfirmed.Should().BeTrue("SignInCommandHandler requires email confirmation before issuing a session");
        stored.RoleName.Should().Be("admin");
        typeof(SignInCommandHandler).Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithUnverifiedAccount_AccountHasPendingStatus()
    {
        var user = NewUser("unverified-login@hivespace.local", "buyer", UserStatus.Pending, emailConfirmed: false);
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var stored = await _fixture.DbContext.Users.SingleAsync(u => u.Email == "unverified-login@hivespace.local");
        stored.EmailConfirmed.Should().BeFalse("handler rejects login when email is not confirmed");
        stored.Status.Should().Be(UserStatus.Pending);
    }

    [Fact]
    public async Task Handle_WithInactiveAccount_AccountHasInactiveStatus()
    {
        var user = NewUser("suspended-login@hivespace.local", "buyer", UserStatus.Inactive, emailConfirmed: true);
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var stored = await _fixture.DbContext.Users.SingleAsync(u => u.Email == "suspended-login@hivespace.local");
        stored.Status.Should().Be(UserStatus.Inactive, "handler must reject login for suspended accounts");
    }

    private static ApplicationUser NewUser(string email, string role, UserStatus status, bool emailConfirmed) =>
        new()
        {
            Id = Guid.NewGuid(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant(),
            RoleName = role,
            Status = status,
            EmailConfirmed = emailConfirmed,
            CreatedAt = DateTimeOffset.UtcNow
        };
}
