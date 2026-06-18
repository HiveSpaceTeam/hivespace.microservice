using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Testing.Shared.Doubles;
using HiveSpace.UserService.Application.Users.Commands.UpdateUserProfile;
using HiveSpace.UserService.Application.Users.Dtos;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Infrastructure.Repositories;
using HiveSpace.UserService.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.UserService.Tests.Application.Profile;

public class UpdateUserProfileCommandHandlerTests : IClassFixture<UserServiceFixture>
{
    private readonly UserServiceFixture _fixture;

    public UpdateUserProfileCommandHandlerTests(UserServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithNewDisplayName_ChangesStoredField()
    {
        var user = NewUser("update-profile@hivespace.local", "Old Name");
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new UpdateUserProfileCommandHandler(
            new FakeUserContext { UserId = user.Id },
            new SqlUserRepository(_fixture.DbContext));

        await handler.Handle(new UpdateUserProfileCommand(new UpdateUserProfileRequestDto(FullName: "New Name")), CancellationToken.None);

        var stored = await _fixture.DbContext.Users.SingleAsync(u => u.Id == user.Id);
        stored.FullName.Should().Be("New Name");
    }

    [Fact]
    public async Task Handle_WithNullName_DoesNotOverrideExistingName()
    {
        var user = NewUser("update-nochange@hivespace.local", "Preserved Name");
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new UpdateUserProfileCommandHandler(
            new FakeUserContext { UserId = user.Id },
            new SqlUserRepository(_fixture.DbContext));

        await handler.Handle(new UpdateUserProfileCommand(new UpdateUserProfileRequestDto()), CancellationToken.None);

        var stored = await _fixture.DbContext.Users.SingleAsync(u => u.Id == user.Id);
        stored.FullName.Should().Be("Preserved Name");
    }

    [Fact]
    public async Task Handle_WithNewUserName_UpdatesStoredUserName()
    {
        var user = NewUser("username-update@hivespace.local", "Test User");
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new UpdateUserProfileCommandHandler(
            new FakeUserContext { UserId = user.Id },
            new SqlUserRepository(_fixture.DbContext));

        await handler.Handle(new UpdateUserProfileCommand(new UpdateUserProfileRequestDto(UserName: "newusername")), CancellationToken.None);

        var stored = await _fixture.DbContext.Users.SingleAsync(u => u.Id == user.Id);
        stored.UserName.Should().Be("newusername");
    }

    [Fact]
    public async Task Handle_WithConflictingUserName_ThrowsConflictException()
    {
        var userA = NewUser("conflict-a@hivespace.local", "User A");
        var userB = NewUser("conflict-b@hivespace.local", "User B");
        _fixture.DbContext.Users.AddRange(userA, userB);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new UpdateUserProfileCommandHandler(
            new FakeUserContext { UserId = userB.Id },
            new SqlUserRepository(_fixture.DbContext));

        var act = () => handler.Handle(
            new UpdateUserProfileCommand(new UpdateUserProfileRequestDto(UserName: userA.UserName)),
            CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WithPhoneNumber_UpdatesStoredPhoneNumber()
    {
        var user = NewUser("phone-update@hivespace.local", "Test User");
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new UpdateUserProfileCommandHandler(
            new FakeUserContext { UserId = user.Id },
            new SqlUserRepository(_fixture.DbContext));

        await handler.Handle(new UpdateUserProfileCommand(new UpdateUserProfileRequestDto(PhoneNumber: "15551234567")), CancellationToken.None);

        var stored = await _fixture.DbContext.Users.SingleAsync(u => u.Id == user.Id);
        stored.PhoneNumber!.Value.Should().Be("15551234567");
    }

    [Fact]
    public async Task Handle_WithDateOfBirth_UpdatesStoredDateOfBirth()
    {
        var user = NewUser("dob-update@hivespace.local", "Test User");
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var dob = DateTimeOffset.UtcNow.AddYears(-25);
        var handler = new UpdateUserProfileCommandHandler(
            new FakeUserContext { UserId = user.Id },
            new SqlUserRepository(_fixture.DbContext));

        await handler.Handle(new UpdateUserProfileCommand(new UpdateUserProfileRequestDto(DateOfBirth: dob)), CancellationToken.None);

        var stored = await _fixture.DbContext.Users.SingleAsync(u => u.Id == user.Id);
        stored.DateOfBirth!.Value.Should().Be(dob);
    }

    [Fact]
    public async Task Handle_WithAvatarFileId_SetsAvatarOnUser()
    {
        var user = NewUser("avatar-update@hivespace.local", "Test User");
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new UpdateUserProfileCommandHandler(
            new FakeUserContext { UserId = user.Id },
            new SqlUserRepository(_fixture.DbContext));

        var act = () => handler.Handle(
            new UpdateUserProfileCommand(new UpdateUserProfileRequestDto(AvatarFileId: "avatar-file-123")),
            CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ThrowsNotFoundException()
    {
        var handler = new UpdateUserProfileCommandHandler(
            new FakeUserContext { UserId = Guid.NewGuid() },
            new SqlUserRepository(_fixture.DbContext));

        var act = () => handler.Handle(
            new UpdateUserProfileCommand(new UpdateUserProfileRequestDto(FullName: "Test")),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    private static User NewUser(string email, string fullName) =>
        User.CreateProfile(Guid.NewGuid(), Email.Create(email), email, fullName);
}
