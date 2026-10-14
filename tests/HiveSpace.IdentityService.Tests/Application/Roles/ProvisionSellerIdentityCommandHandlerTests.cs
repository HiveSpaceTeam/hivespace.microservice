using FluentAssertions;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Features.Roles.Commands.ProvisionSellerIdentity;
using HiveSpace.IdentityService.Tests.Fixtures;
using HiveSpace.IdentityService.Tests.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace HiveSpace.IdentityService.Tests.Application.Roles;

public class ProvisionSellerIdentityCommandHandlerTests : IClassFixture<IdentityServiceFixture>
{
    private readonly IdentityServiceFixture _fixture;

    public ProvisionSellerIdentityCommandHandlerTests(IdentityServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_NewUserStore_GrantsSellerRoleAndStoreReference()
    {
        var user = NewUser("provision-seller-created");
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var storeId = Guid.NewGuid();
        var userManager = IdentityMocks.UserManager();
        userManager.IsInRoleAsync(user, "Seller").Returns(false);
        userManager.AddToRoleAsync(user, "Seller").Returns(IdentityResult.Success);

        var handler = new ProvisionSellerIdentityCommandHandler(_fixture.DbContext, userManager);
        var result = await handler.Handle(NewCommand(user.Id, storeId), CancellationToken.None);

        result.Outcome.Should().Be(ProvisionSellerIdentityOutcome.Created);
        result.IdentityUserId.Should().Be(user.Id);
        var stored = await _fixture.DbContext.Users.SingleAsync(u => u.Id == user.Id);
        stored.RoleName.Should().Be("Seller");
        stored.StoreId.Should().Be(storeId);
    }

    [Fact]
    public async Task Handle_SameUserStore_ReturnsMatchedWithoutDuplicateRoleOrClaims()
    {
        var storeId = Guid.NewGuid();
        var user = NewUser("provision-seller-matched", "Seller", storeId);
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var userManager = IdentityMocks.UserManager();
        var handler = new ProvisionSellerIdentityCommandHandler(_fixture.DbContext, userManager);

        var result = await handler.Handle(NewCommand(user.Id, storeId), CancellationToken.None);

        result.Outcome.Should().Be(ProvisionSellerIdentityOutcome.Matched);
        await userManager.DidNotReceive().IsInRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>());
        await userManager.DidNotReceive().AddToRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_DifferentStoreConflict_ReturnsConflict()
    {
        var user = NewUser("provision-seller-conflict", "Seller", Guid.NewGuid());
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new ProvisionSellerIdentityCommandHandler(_fixture.DbContext, IdentityMocks.UserManager());

        var result = await handler.Handle(NewCommand(user.Id, Guid.NewGuid()), CancellationToken.None);

        result.Outcome.Should().Be(ProvisionSellerIdentityOutcome.Conflict);
        result.FailureCode.Should().Be("SellerStoreConflict");
    }

    [Fact]
    public async Task Handle_UnknownUser_ReturnsFailed()
    {
        var handler = new ProvisionSellerIdentityCommandHandler(_fixture.DbContext, IdentityMocks.UserManager());

        var result = await handler.Handle(NewCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        result.Outcome.Should().Be(ProvisionSellerIdentityOutcome.Failed);
        result.FailureCode.Should().Be("IdentityUserNotFound");
    }

    [Fact]
    public async Task Handle_RoleAssignmentFails_ReturnsFailed()
    {
        var user = NewUser("provision-seller-role-fails");
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var userManager = IdentityMocks.UserManager();
        userManager.IsInRoleAsync(user, "Seller").Returns(false);
        userManager.AddToRoleAsync(user, "Seller")
            .Returns(IdentityResult.Failed(new IdentityError { Code = "RoleMissing" }));

        var handler = new ProvisionSellerIdentityCommandHandler(_fixture.DbContext, userManager);
        var result = await handler.Handle(NewCommand(user.Id, Guid.NewGuid()), CancellationToken.None);

        result.Outcome.Should().Be(ProvisionSellerIdentityOutcome.Failed);
        result.FailureCode.Should().Be("SellerRoleAssignmentFailed");
    }

    [Fact]
    public async Task Handle_SellerWithoutStoreReference_AssignsStoreWithoutAddingRoleAgain()
    {
        var user = NewUser("provision-seller-no-store", "Seller");
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var storeId = Guid.NewGuid();
        var userManager = IdentityMocks.UserManager();
        userManager.IsInRoleAsync(user, "Seller").Returns(true);

        var handler = new ProvisionSellerIdentityCommandHandler(_fixture.DbContext, userManager);
        var result = await handler.Handle(NewCommand(user.Id, storeId), CancellationToken.None);

        result.Outcome.Should().Be(ProvisionSellerIdentityOutcome.Created);
        (await _fixture.DbContext.Users.SingleAsync(u => u.Id == user.Id)).StoreId.Should().Be(storeId);
        await userManager.DidNotReceive().AddToRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>());
    }

    private static ProvisionSellerIdentityCommand NewCommand(Guid userId, Guid storeId)
        => new(userId, storeId, "Store", Guid.NewGuid().ToString("N"));

    private static ApplicationUser NewUser(
        string name,
        string roleName = "Buyer",
        Guid? storeId = null)
        => new()
        {
            Id = Guid.NewGuid(),
            Email = $"{name}-{Guid.NewGuid()}@hivespace.local",
            NormalizedEmail = $"{name}-{Guid.NewGuid()}@HIVESPACE.LOCAL".ToUpperInvariant(),
            UserName = $"{name}-{Guid.NewGuid()}@hivespace.local",
            NormalizedUserName = $"{name}-{Guid.NewGuid()}@HIVESPACE.LOCAL".ToUpperInvariant(),
            RoleName = roleName,
            StoreId = storeId,
            Status = UserStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
}
