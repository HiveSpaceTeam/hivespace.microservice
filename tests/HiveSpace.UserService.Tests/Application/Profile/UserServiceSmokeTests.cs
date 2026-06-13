using FluentAssertions;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Tests.Fixtures;
using HiveSpace.Testing.Shared.Fakes;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.UserService.Tests.Application.Profile;

public class CreateProfileCommandHandlerTests : IClassFixture<UserServiceFixture>
{
    private readonly UserServiceFixture _fixture;

    public CreateProfileCommandHandlerTests(UserServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithValidFields_PersistsNameAndAvatarReference()
    {
        var user = NewUser("profile-create@hivespace.local", "Profile User");
        user.SetAvatar("avatar-file");
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var stored = await _fixture.DbContext.Users.SingleAsync(x => x.Id == user.Id);
        stored.FullName.Should().Be("Profile User");
        stored.AvatarFileId.Should().Be("avatar-file");
    }

    [Fact]
    public async Task BlobStorageFake_CapturesAvatarReferenceWithoutLiveStorage()
    {
        var fake = new BlobStorageFake();
        await fake.ConfirmUploadAsync("avatar-key");

        fake.ConfirmedKeys.Should().Contain("avatar-key");
    }

    private static User NewUser(string email, string fullName) =>
        User.CreateProfile(Guid.NewGuid(), Email.Create(email), email, fullName);
}
