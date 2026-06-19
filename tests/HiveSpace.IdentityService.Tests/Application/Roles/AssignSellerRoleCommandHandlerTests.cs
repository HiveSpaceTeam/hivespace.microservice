using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Features.Roles.Commands.AssignSellerRole;
using HiveSpace.IdentityService.Tests.Fixtures;
using HiveSpace.IdentityService.Tests.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace HiveSpace.IdentityService.Tests.Application.Roles;

public class AssignSellerRoleCommandHandlerTests : IClassFixture<IdentityServiceFixture>
{
    private readonly IdentityServiceFixture _fixture;

    public AssignSellerRoleCommandHandlerTests(IdentityServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithBuyerUser_AssignsSellerRoleAndStoreId()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = $"assign-seller-{Guid.NewGuid()}@hivespace.local",
            NormalizedEmail = "ASSIGN@HIVESPACE.LOCAL",
            UserName = "assign-seller@hivespace.local",
            NormalizedUserName = "ASSIGN-SELLER@HIVESPACE.LOCAL",
            RoleName = "Buyer",
            Status = UserStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var storeId = Guid.NewGuid();
        var userManager = IdentityMocks.UserManager();
        userManager.IsInRoleAsync(user, "Seller").Returns(false);
        userManager.AddToRoleAsync(user, "Seller").Returns(IdentityResult.Success);

        var handler = new AssignSellerRoleCommandHandler(_fixture.DbContext, userManager);
        await handler.Handle(new AssignSellerRoleCommand(user.Id, storeId), CancellationToken.None);

        var stored = await _fixture.DbContext.Users.SingleAsync(u => u.Id == user.Id);
        stored.RoleName.Should().Be("Seller");
        stored.StoreId.Should().Be(storeId);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ThrowsNotFoundException()
    {
        var handler = new AssignSellerRoleCommandHandler(_fixture.DbContext, IdentityMocks.UserManager());

        var act = () => handler.Handle(
            new AssignSellerRoleCommand(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenUserAlreadySellerForStore_ReturnsWithoutAddingRole()
    {
        var storeId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = $"existing-seller-{Guid.NewGuid()}@hivespace.local",
            UserName = $"existing-seller-{Guid.NewGuid()}@hivespace.local",
            RoleName = "Seller",
            StoreId = storeId,
            Status = UserStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var userManager = IdentityMocks.UserManager();
        var handler = new AssignSellerRoleCommandHandler(_fixture.DbContext, userManager);

        await handler.Handle(new AssignSellerRoleCommand(user.Id, storeId), CancellationToken.None);

        await userManager.DidNotReceive().IsInRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>());
        await userManager.DidNotReceive().AddToRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_WhenAddToRoleFails_ThrowsConflictException()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = $"seller-conflict-{Guid.NewGuid()}@hivespace.local",
            UserName = $"seller-conflict-{Guid.NewGuid()}@hivespace.local",
            RoleName = "Buyer",
            Status = UserStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var userManager = IdentityMocks.UserManager();
        userManager.IsInRoleAsync(user, "Seller").Returns(false);
        userManager.AddToRoleAsync(user, "Seller")
            .Returns(IdentityResult.Failed(new IdentityError { Code = "RoleMissing" }));

        var handler = new AssignSellerRoleCommandHandler(_fixture.DbContext, userManager);
        var act = () => handler.Handle(
            new AssignSellerRoleCommand(user.Id, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenSellerReassignedToDifferentStore_UpdatesStoreId()
    {
        var oldStoreId = Guid.NewGuid();
        var newStoreId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = $"reassign-seller-{Guid.NewGuid()}@hivespace.local",
            UserName = $"reassign-seller-{Guid.NewGuid()}@hivespace.local",
            RoleName = "Seller",
            StoreId = oldStoreId,
            Status = UserStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var userManager = IdentityMocks.UserManager();
        userManager.IsInRoleAsync(user, "Seller").Returns(true);

        var handler = new AssignSellerRoleCommandHandler(_fixture.DbContext, userManager);
        await handler.Handle(new AssignSellerRoleCommand(user.Id, newStoreId), CancellationToken.None);

        var stored = await _fixture.DbContext.Users.SingleAsync(u => u.Id == user.Id);
        stored.StoreId.Should().Be(newStoreId);
        await userManager.DidNotReceive().AddToRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_WhenSellerHasNoStoreAssigned_AssignsNewStore()
    {
        var newStoreId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = $"seller-nostore-{Guid.NewGuid()}@hivespace.local",
            UserName = $"seller-nostore-{Guid.NewGuid()}@hivespace.local",
            RoleName = "Seller",
            StoreId = null,
            Status = UserStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var userManager = IdentityMocks.UserManager();
        userManager.IsInRoleAsync(user, "Seller").Returns(true);

        var handler = new AssignSellerRoleCommandHandler(_fixture.DbContext, userManager);
        await handler.Handle(new AssignSellerRoleCommand(user.Id, newStoreId), CancellationToken.None);

        var stored = await _fixture.DbContext.Users.SingleAsync(u => u.Id == user.Id);
        stored.StoreId.Should().Be(newStoreId);
    }
}
