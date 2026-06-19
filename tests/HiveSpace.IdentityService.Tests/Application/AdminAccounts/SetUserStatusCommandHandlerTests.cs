using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Features.AdminIdentity.Commands.SetUserStatus;
using HiveSpace.IdentityService.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.IdentityService.Tests.Application.AdminAccounts;

public class SetUserStatusCommandHandlerTests : IClassFixture<IdentityServiceFixture>
{
    private readonly IdentityServiceFixture _fixture;

    public SetUserStatusCommandHandlerTests(IdentityServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_Suspend_SetsStatusToInactive()
    {
        var user = NewUser($"sus-{Guid.NewGuid()}@hivespace.local", UserStatus.Active);
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new SetUserStatusCommandHandler(_fixture.DbContext);
        await handler.Handle(new SetUserStatusCommand(user.Id, IsActive: false), CancellationToken.None);

        var stored = await _fixture.DbContext.Users.SingleAsync(u => u.Id == user.Id);
        stored.Status.Should().Be(UserStatus.Inactive);
    }

    [Fact]
    public async Task Handle_Activate_SetsStatusToActive()
    {
        var user = NewUser($"act-{Guid.NewGuid()}@hivespace.local", UserStatus.Inactive);
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new SetUserStatusCommandHandler(_fixture.DbContext);
        await handler.Handle(new SetUserStatusCommand(user.Id, IsActive: true), CancellationToken.None);

        var stored = await _fixture.DbContext.Users.SingleAsync(u => u.Id == user.Id);
        stored.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public async Task Handle_WithUnknownUser_ThrowsNotFoundException()
    {
        var handler = new SetUserStatusCommandHandler(_fixture.DbContext);
        var act = () => handler.Handle(new SetUserStatusCommand(Guid.NewGuid(), IsActive: false), CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
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
            FullName = "Status User",
            CreatedAt = DateTimeOffset.UtcNow
        };
}
