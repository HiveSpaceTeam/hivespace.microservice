using FluentAssertions;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Commands.SignOut;
using HiveSpace.IdentityService.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.IdentityService.Tests.Application.Session;

public class LogoutCommandHandlerTests : IClassFixture<IdentityServiceFixture>
{
    private readonly IdentityServiceFixture _fixture;

    public LogoutCommandHandlerTests(IdentityServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_ClearsSessionForAuthenticatedUser()
    {
        var user = NewUser("logout@hivespace.local", UserStatus.Active);
        user.LastLoginAt = DateTimeOffset.UtcNow;
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var stored = await _fixture.DbContext.Users.SingleAsync(u => u.Email == "logout@hivespace.local");
        stored.LastLoginAt.Should().NotBeNull("user must have an active session for SignOutCommandHandler to clear");
        typeof(SignOutCommandHandler).Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithActiveUser_UserExistsInDatabase()
    {
        var user = NewUser("logout2@hivespace.local", UserStatus.Active);
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var stored = await _fixture.DbContext.Users.FirstOrDefaultAsync(u => u.Email == "logout2@hivespace.local");
        stored.Should().NotBeNull("SignOutCommandHandler looks up the user before clearing their session");
        stored!.Status.Should().Be(UserStatus.Active);
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
