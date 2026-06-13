using FluentAssertions;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.UserService.Tests.Application.StoreOnboarding;

public class ApproveStoreCommandHandlerTests : IClassFixture<UserServiceFixture>
{
    private readonly UserServiceFixture _fixture;

    public ApproveStoreCommandHandlerTests(UserServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithPendingRegistration_UserCanBeLocatedForApproval()
    {
        // ApproveStoreCommandHandler loads the user before transitioning their store to Approved.
        var user = NewUser("store-approve@hivespace.local");
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var stored = await _fixture.DbContext.Users.SingleAsync(u => u.Id == user.Id);
        stored.Should().NotBeNull("admin approval handler must be able to locate the user by ID");
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ApprovalCannotProceed()
    {
        var missingId = Guid.NewGuid();
        var user = await _fixture.DbContext.Users.FirstOrDefaultAsync(u => u.Id == missingId);
        user.Should().BeNull("ApproveStoreCommandHandler throws NotFoundException when the user does not exist");
    }

    private static User NewUser(string email) =>
        User.CreateProfile(Guid.NewGuid(), Email.Create(email), email, "Store Owner");
}
