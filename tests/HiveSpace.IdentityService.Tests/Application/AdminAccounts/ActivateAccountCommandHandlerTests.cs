using FluentAssertions;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Features.AdminIdentity.Commands.SetUserStatus;
using HiveSpace.IdentityService.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.IdentityService.Tests.Application.AdminAccounts;

public class ActivateAccountCommandHandlerTests : IClassFixture<IdentityServiceFixture>
{
    private readonly IdentityServiceFixture _fixture;

    public ActivateAccountCommandHandlerTests(IdentityServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithInactiveAccount_SetsStatusToActive()
    {
        var user = NewUser("activate@hivespace.local", UserStatus.Inactive);
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new SetUserStatusCommandHandler(_fixture.DbContext);
        await handler.Handle(new SetUserStatusCommand(user.Id, IsActive: true), CancellationToken.None);

        var stored = await _fixture.DbContext.Users.SingleAsync(u => u.Id == user.Id);
        stored.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public async Task Handle_ActivationUpdatesTimestamp()
    {
        var user = NewUser("activate-ts@hivespace.local", UserStatus.Inactive);
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new SetUserStatusCommandHandler(_fixture.DbContext);
        await handler.Handle(new SetUserStatusCommand(user.Id, IsActive: true), CancellationToken.None);

        var stored = await _fixture.DbContext.Users.SingleAsync(u => u.Id == user.Id);
        stored.UpdatedAt.Should().NotBeNull("SetUserStatusCommandHandler sets UpdatedAt on every status change");
        stored.Status.Should().Be(UserStatus.Active);
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
