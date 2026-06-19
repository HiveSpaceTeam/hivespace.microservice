using FluentAssertions;
using HiveSpace.Core.Exceptions;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Features.AdminIdentity.Commands.CreateAdmin;
using HiveSpace.IdentityService.Tests.Fixtures;
using HiveSpace.IdentityService.Tests.Helpers;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Xunit;

namespace HiveSpace.IdentityService.Tests.Application.AdminAccounts;

public class CreateAdminCommandHandlerTests : IClassFixture<IdentityServiceFixture>
{
    private readonly IdentityServiceFixture _fixture;

    public CreateAdminCommandHandlerTests(IdentityServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithValidInput_CreatesAdminAndReturnsResult()
    {
        var userManager = IdentityMocks.UserManager();
        userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(IdentityResult.Success);
        userManager.AddToRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(IdentityResult.Success);

        var handler = new CreateAdminCommandHandler(userManager, _fixture.DbContext);

        var result = await handler.Handle(
            new CreateAdminCommand("admin-new@hivespace.local", "Adm1n@Pass", "New Admin", "Adm1n@Pass", false),
            CancellationToken.None);

        result.Should().NotBeNull();
        result.Email.Should().Be("admin-new@hivespace.local");
    }

    [Fact]
    public async Task Handle_WithPasswordMismatch_ThrowsBadRequestException()
    {
        var handler = new CreateAdminCommandHandler(IdentityMocks.UserManager(), _fixture.DbContext);

        var act = () => handler.Handle(
            new CreateAdminCommand("admin-mismatch@hivespace.local", "P@ss1", "Admin", "P@ss2", false),
            CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WithIsSystemAdmin_AssignsSystemAdminRole()
    {
        var userManager = IdentityMocks.UserManager();
        userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(IdentityResult.Success);
        userManager.AddToRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(IdentityResult.Success);

        var handler = new CreateAdminCommandHandler(userManager, _fixture.DbContext);

        var result = await handler.Handle(
            new CreateAdminCommand("sysadmin@hivespace.local", "Adm1n@Pass", "System Admin", "Adm1n@Pass", true),
            CancellationToken.None);

        result.IsSystemAdmin.Should().BeTrue();
        await userManager.Received(1).AddToRoleAsync(Arg.Any<ApplicationUser>(), "SystemAdmin");
    }

    [Fact]
    public async Task Handle_WhenRoleAssignmentFails_ThrowsBadRequestException()
    {
        var userManager = IdentityMocks.UserManager();
        userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(IdentityResult.Success);
        userManager.AddToRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(IdentityResult.Failed(new IdentityError { Code = "RoleAssignFailed" }));

        var handler = new CreateAdminCommandHandler(userManager, _fixture.DbContext);

        var act = () => handler.Handle(
            new CreateAdminCommand("roleonly@hivespace.local", "Adm1n@Pass", "Admin", "Adm1n@Pass", false),
            CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenUserCreateFails_ThrowsBadRequestException()
    {
        var userManager = IdentityMocks.UserManager();
        userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(IdentityResult.Failed(new IdentityError { Code = "DuplicateUserName" }));

        var handler = new CreateAdminCommandHandler(userManager, _fixture.DbContext);

        var act = () => handler.Handle(
            new CreateAdminCommand("fail-create@hivespace.local", "Adm1n@Pass", "Admin", "Adm1n@Pass", false),
            CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }
}
