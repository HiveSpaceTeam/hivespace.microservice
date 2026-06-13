using FluentAssertions;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.UserService.Tests.Application.Profile;

public class UpdateProfileCommandHandlerTests : IClassFixture<UserServiceFixture>
{
    private readonly UserServiceFixture _fixture;

    public UpdateProfileCommandHandlerTests(UserServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithNewDisplayName_ChangesStoredField()
    {
        var user = NewUser("update-profile@hivespace.local", "Old Name");
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        user.UpdateProfile("New Name", null, null, null);
        await _fixture.DbContext.SaveChangesAsync();

        user.FullName.Should().Be("New Name");
    }

    [Fact]
    public async Task Handle_WithNullName_DoesNotOverrideExistingName()
    {
        var user = NewUser("update-nochange@hivespace.local", "Preserved Name");
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        user.UpdateProfile(null, null, null, null);
        await _fixture.DbContext.SaveChangesAsync();

        var stored = await _fixture.DbContext.Users.SingleAsync(u => u.Id == user.Id);
        stored.FullName.Should().Be("Preserved Name");
    }

    private static User NewUser(string email, string fullName) =>
        User.CreateProfile(Guid.NewGuid(), Email.Create(email), email, fullName);
}
