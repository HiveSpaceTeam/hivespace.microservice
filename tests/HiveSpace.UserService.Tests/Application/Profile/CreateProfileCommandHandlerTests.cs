using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Testing.Shared.Doubles;
using HiveSpace.UserService.Application.Users.Queries.GetUserProfile;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Infrastructure.Repositories;
using HiveSpace.UserService.Tests.Fixtures;
using Xunit;

namespace HiveSpace.UserService.Tests.Application.Profile;

public class GetUserProfileQueryHandlerTests : IClassFixture<UserServiceFixture>
{
    private readonly UserServiceFixture _fixture;

    public GetUserProfileQueryHandlerTests(UserServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithExistingUser_ReturnsStoredProfile()
    {
        var user = NewUser("profile-create@hivespace.local", "Profile User");
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new GetUserProfileQueryHandler(
            new FakeUserContext { UserId = user.Id },
            new SqlUserRepository(_fixture.DbContext));

        var profile = await handler.Handle(new GetUserProfileQuery(), CancellationToken.None);

        profile.FullName.Should().Be("Profile User");
        profile.Email.Should().Be("profile-create@hivespace.local");
    }

    [Fact]
    public async Task Handle_WithAvatarFileId_AvatarUrlIsNull()
    {
        var user = NewUser("profile-avatar@hivespace.local", "Avatar User");
        user.SetAvatar("avatar-key");
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new GetUserProfileQueryHandler(
            new FakeUserContext { UserId = user.Id },
            new SqlUserRepository(_fixture.DbContext));

        var profile = await handler.Handle(new GetUserProfileQuery(), CancellationToken.None);

        profile.AvatarUrl.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithPhoneNumberAndDateOfBirth_MapsBothFields()
    {
        var phone = PhoneNumber.Create("15551234567");
        var dob = DateOfBirth.Create(DateTimeOffset.UtcNow.AddYears(-25));
        var user = User.CreateProfile(
            Guid.NewGuid(), Email.Create("profile-full@hivespace.local"), "profilefull", "Full User",
            phoneNumber: phone, dateOfBirth: dob);
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new GetUserProfileQueryHandler(
            new FakeUserContext { UserId = user.Id },
            new SqlUserRepository(_fixture.DbContext));

        var profile = await handler.Handle(new GetUserProfileQuery(), CancellationToken.None);

        profile.PhoneNumber.Should().NotBeNullOrEmpty();
        profile.DateOfBirth.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ThrowsNotFoundException()
    {
        var handler = new GetUserProfileQueryHandler(
            new FakeUserContext { UserId = Guid.NewGuid() },
            new SqlUserRepository(_fixture.DbContext));

        var act = () => handler.Handle(new GetUserProfileQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    private static User NewUser(string email, string fullName) =>
        User.CreateProfile(Guid.NewGuid(), Email.Create(email), email, fullName);
}
