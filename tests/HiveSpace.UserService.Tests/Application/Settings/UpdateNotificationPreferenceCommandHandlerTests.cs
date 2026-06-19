using FluentAssertions;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Testing.Shared.Doubles;
using HiveSpace.UserService.Application.Users.Commands.UpdateUserSetting;
using HiveSpace.UserService.Application.Users.Dtos;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Enums;
using HiveSpace.UserService.Infrastructure.Repositories;
using HiveSpace.UserService.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.UserService.Tests.Application.Settings;

public class UpdateUserSettingCommandHandlerTests : IClassFixture<UserServiceFixture>
{
    private readonly UserServiceFixture _fixture;

    public UpdateUserSettingCommandHandlerTests(UserServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_UpdateTheme_WithDark_PersistsDarkTheme()
    {
        var user = NewUser("theme-dark@hivespace.local");
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new UpdateUserSettingCommandHandler(
            new FakeUserContext { UserId = user.Id },
            new SqlUserRepository(_fixture.DbContext));

        await handler.Handle(new UpdateUserSettingCommand(new UpdateUserSettingRequestDto(Theme: "dark")), CancellationToken.None);

        var stored = await _fixture.DbContext.Users.SingleAsync(u => u.Id == user.Id);
        stored.Settings.Theme.Should().Be(Theme.Dark);
    }

    [Fact]
    public async Task Handle_UpdateTheme_WithLight_PersistsLightTheme()
    {
        var user = NewUser("theme-light@hivespace.local");
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new UpdateUserSettingCommandHandler(
            new FakeUserContext { UserId = user.Id },
            new SqlUserRepository(_fixture.DbContext));

        await handler.Handle(new UpdateUserSettingCommand(new UpdateUserSettingRequestDto(Theme: "light")), CancellationToken.None);

        var stored = await _fixture.DbContext.Users.SingleAsync(u => u.Id == user.Id);
        stored.Settings.Theme.Should().Be(Theme.Light);
    }

    [Fact]
    public async Task Handle_UpdateCulture_WithEnglish_PersistsEnCulture()
    {
        var user = NewUser("culture-en@hivespace.local");
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new UpdateUserSettingCommandHandler(
            new FakeUserContext { UserId = user.Id },
            new SqlUserRepository(_fixture.DbContext));

        await handler.Handle(new UpdateUserSettingCommand(new UpdateUserSettingRequestDto(Culture: "en")), CancellationToken.None);

        var stored = await _fixture.DbContext.Users.SingleAsync(u => u.Id == user.Id);
        stored.Settings.Culture.Should().Be(Culture.En);
    }

    [Fact]
    public async Task Handle_UpdateCulture_WithVietnamese_PersistsViCulture()
    {
        var user = NewUser("culture-vi@hivespace.local");
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new UpdateUserSettingCommandHandler(
            new FakeUserContext { UserId = user.Id },
            new SqlUserRepository(_fixture.DbContext));

        await handler.Handle(new UpdateUserSettingCommand(new UpdateUserSettingRequestDto(Culture: "vi")), CancellationToken.None);

        var stored = await _fixture.DbContext.Users.SingleAsync(u => u.Id == user.Id);
        stored.Settings.Culture.Should().Be(Culture.Vi);
    }

    [Fact]
    public async Task Handle_UpdateTheme_WithInvalidValue_ThrowsInvalidFieldException()
    {
        var user = NewUser("theme-invalid@hivespace.local");
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new UpdateUserSettingCommandHandler(
            new FakeUserContext { UserId = user.Id },
            new SqlUserRepository(_fixture.DbContext));

        var act = () => handler.Handle(new UpdateUserSettingCommand(new UpdateUserSettingRequestDto(Theme: "invalid")), CancellationToken.None);

        await act.Should().ThrowAsync<HiveSpace.Domain.Shared.Exceptions.InvalidFieldException>();
    }

    [Fact]
    public async Task Handle_UpdateCulture_WithInvalidValue_ThrowsInvalidFieldException()
    {
        var user = NewUser("culture-invalid@hivespace.local");
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new UpdateUserSettingCommandHandler(
            new FakeUserContext { UserId = user.Id },
            new SqlUserRepository(_fixture.DbContext));

        var act = () => handler.Handle(new UpdateUserSettingCommand(new UpdateUserSettingRequestDto(Culture: "invalid")), CancellationToken.None);

        await act.Should().ThrowAsync<HiveSpace.Domain.Shared.Exceptions.InvalidFieldException>();
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ThrowsNotFoundException()
    {
        var handler = new UpdateUserSettingCommandHandler(
            new FakeUserContext { UserId = Guid.NewGuid() },
            new SqlUserRepository(_fixture.DbContext));

        var act = () => handler.Handle(
            new UpdateUserSettingCommand(new UpdateUserSettingRequestDto(Theme: "dark")),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    private static User NewUser(string email) =>
        User.CreateProfile(Guid.NewGuid(), Email.Create(email), email, "Test User");
}
