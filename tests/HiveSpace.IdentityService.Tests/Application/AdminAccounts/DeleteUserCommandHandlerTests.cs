using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Features.AdminIdentity.Commands.DeleteUser;
using HiveSpace.IdentityService.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.IdentityService.Tests.Application.AdminAccounts;

public class DeleteUserCommandHandlerTests : IClassFixture<IdentityServiceFixture>
{
    private readonly IdentityServiceFixture _fixture;

    public DeleteUserCommandHandlerTests(IdentityServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithExistingUser_SetsStatusToInactiveAndReturnsId()
    {
        var user = NewUser($"del-{Guid.NewGuid()}@hivespace.local");
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new DeleteUserCommandHandler(_fixture.DbContext);
        var result = await handler.Handle(new DeleteUserCommand(user.Id, "admin"), CancellationToken.None);

        result.Id.Should().Be(user.Id);
        var stored = await _fixture.DbContext.Users.SingleAsync(u => u.Id == user.Id);
        stored.Status.Should().Be(UserStatus.Inactive);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ThrowsNotFoundException()
    {
        var handler = new DeleteUserCommandHandler(_fixture.DbContext);
        var act = () => handler.Handle(new DeleteUserCommand(Guid.NewGuid(), null), CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenUserHasNullUsernameAndEmail_ReturnsEmptyStrings()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = null,
            Email = null,
            RoleName = "Buyer",
            Status = UserStatus.Active,
            FullName = "Test User",
            CreatedAt = DateTimeOffset.UtcNow
        };
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new DeleteUserCommandHandler(_fixture.DbContext);
        var result = await handler.Handle(new DeleteUserCommand(user.Id, "admin"), CancellationToken.None);

        result.Username.Should().BeEmpty();
        result.Email.Should().BeEmpty();
    }

    private static ApplicationUser NewUser(string email) =>
        new()
        {
            Id = Guid.NewGuid(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant(),
            RoleName = "buyer",
            Status = UserStatus.Active,
            FullName = "Test User",
            CreatedAt = DateTimeOffset.UtcNow
        };
}
