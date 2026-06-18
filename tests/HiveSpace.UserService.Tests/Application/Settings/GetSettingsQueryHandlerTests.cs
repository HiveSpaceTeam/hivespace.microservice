using FluentAssertions;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Testing.Shared.Doubles;
using HiveSpace.UserService.Application.Users.Commands.UpdateUserSetting;
using HiveSpace.UserService.Application.Users.Dtos;
using HiveSpace.UserService.Application.Users.Queries.GetUserSetting;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Enums;
using HiveSpace.UserService.Infrastructure.Repositories;
using HiveSpace.UserService.Tests.Fixtures;
using Xunit;

namespace HiveSpace.UserService.Tests.Application.Settings;

public class GetUserSettingQueryHandlerTests : IClassFixture<UserServiceFixture>
{
    private readonly UserServiceFixture _fixture;

    public GetUserSettingQueryHandlerTests(UserServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_ReturnsCurrentThemeAndCulture()
    {
        var user = NewUser("settings-get@hivespace.local");
        user.UpdateTheme(Theme.Dark);
        user.UpdateCulture(Culture.En);
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new GetUserSettingQueryHandler(
            new FakeUserContext { UserId = user.Id },
            new SqlUserRepository(_fixture.DbContext));

        var settings = await handler.Handle(new GetUserSettingQuery(), CancellationToken.None);

        settings.Theme.Should().Be("dark");
        settings.Culture.Should().Be("en");
    }

    [Fact]
    public async Task Handle_DefaultSettings_ReturnsSystemDefaultValues()
    {
        var user = NewUser("settings-default@hivespace.local");
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new GetUserSettingQueryHandler(
            new FakeUserContext { UserId = user.Id },
            new SqlUserRepository(_fixture.DbContext));

        var settings = await handler.Handle(new GetUserSettingQuery(), CancellationToken.None);

        settings.Theme.Should().NotBeNullOrWhiteSpace();
        settings.Culture.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Handle_AfterSettingViCulture_ReturnsCultureCodeVi()
    {
        var user = NewUser("settings-vi@hivespace.local");
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var repo = new SqlUserRepository(_fixture.DbContext);
        var updateHandler = new UpdateUserSettingCommandHandler(new FakeUserContext { UserId = user.Id }, repo);
        await updateHandler.Handle(new UpdateUserSettingCommand(new UpdateUserSettingRequestDto(Culture: "vi")), CancellationToken.None);

        var getHandler = new GetUserSettingQueryHandler(new FakeUserContext { UserId = user.Id }, repo);
        var settings = await getHandler.Handle(new GetUserSettingQuery(), CancellationToken.None);

        settings.Culture.Should().Be("vi");
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ThrowsNotFoundException()
    {
        var handler = new GetUserSettingQueryHandler(
            new FakeUserContext { UserId = Guid.NewGuid() },
            new SqlUserRepository(_fixture.DbContext));

        var act = () => handler.Handle(new GetUserSettingQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    private static User NewUser(string email) =>
        User.CreateProfile(Guid.NewGuid(), Email.Create(email), email, "Test User");
}
