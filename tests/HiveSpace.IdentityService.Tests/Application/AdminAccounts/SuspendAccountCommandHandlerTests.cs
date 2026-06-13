using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Features.AdminIdentity.Commands.SetUserStatus;
using HiveSpace.IdentityService.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.IdentityService.Tests.Application.AdminAccounts;

public class SuspendAccountCommandHandlerTests : IClassFixture<IdentityServiceFixture>
{
    private readonly IdentityServiceFixture _fixture;

    public SuspendAccountCommandHandlerTests(IdentityServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithActiveAccount_SetsStatusToInactive()
    {
        var user = NewUser("suspend@hivespace.local", UserStatus.Active);
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new SetUserStatusCommandHandler(_fixture.DbContext);
        await handler.Handle(new SetUserStatusCommand(user.Id, IsActive: false), CancellationToken.None);

        var stored = await _fixture.DbContext.Users.SingleAsync(u => u.Id == user.Id);
        stored.Status.Should().Be(UserStatus.Inactive);
    }

    [Fact]
    public async Task Handle_WithUnknownUserId_ThrowsNotFoundException()
    {
        var handler = new SetUserStatusCommandHandler(_fixture.DbContext);
        Func<Task> act = async () => await handler.Handle(
            new SetUserStatusCommand(Guid.NewGuid(), IsActive: false), CancellationToken.None);
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
            CreatedAt = DateTimeOffset.UtcNow
        };
}
