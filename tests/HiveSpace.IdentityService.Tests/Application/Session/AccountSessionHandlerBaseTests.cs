using System.Reflection;
using FluentAssertions;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Dtos;
using HiveSpace.IdentityService.Tests.Helpers;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Xunit;

namespace HiveSpace.IdentityService.Tests.Application.Session;

public class AccountSessionHandlerBaseTests
{
    private static readonly Type Subject = typeof(HiveSpace.IdentityService.Core.Features.AccountSessions.Commands.SignIn.SignInCommandHandler)
        .Assembly
        .GetType("HiveSpace.IdentityService.Core.Features.AccountSessions.Commands.AccountSessionHandlerBase", throwOnError: true)!;

    [Fact]
    public void NormalizeApp_TrimAndLowercase()
    {
        Invoke<string>("NormalizeApp", " Seller ").Should().Be("seller");
    }

    [Theory]
    [InlineData("admin", new[] { "Admin" }, true)]
    [InlineData("admin", new[] { "SystemAdmin" }, true)]
    [InlineData("seller", new[] { "Buyer" }, true)]
    [InlineData("seller", new[] { "Admin" }, false)]
    [InlineData("buyer", new[] { "Buyer" }, true)]
    [InlineData("unknown", new[] { "Buyer" }, false)]
    public void UserCanAccessApp_UsesExpectedRules(string app, string[] roles, bool expected)
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "roles@hivespace.local" };

        Invoke<bool>("UserCanAccessApp", user, app, new HashSet<string>(roles)).Should().Be(expected);
    }

    [Fact]
    public async Task GetRolesAsync_UsesRoleName()
    {
        var user = new ApplicationUser { RoleName = "Seller" };
        var userManager = IdentityMocks.UserManager();
        userManager.GetRolesAsync(user).Returns([]);

        var roles = await InvokeTask<IReadOnlySet<string>>("GetRolesAsync", userManager, user);

        roles.Should().BeEquivalentTo(["Seller"]);
    }

    [Fact]
    public async Task GetRolesAsync_MergesRoleNameAndUserManagerRolesWithoutDuplicateCasing()
    {
        var user = new ApplicationUser { RoleName = "Buyer" };
        var userManager = IdentityMocks.UserManager();
        userManager.GetRolesAsync(user).Returns(["buyer", "Seller"]);

        var roles = await InvokeTask<IReadOnlySet<string>>("GetRolesAsync", userManager, user);

        roles.Should().BeEquivalentTo(["Buyer", "Seller"]);
    }

    [Fact]
    public async Task GetRolesAsync_DefaultsToBuyerWhenNoRolesExist()
    {
        var user = new ApplicationUser();
        var userManager = IdentityMocks.UserManager();
        userManager.GetRolesAsync(user).Returns([]);

        var roles = await InvokeTask<IReadOnlySet<string>>("GetRolesAsync", userManager, user);

        roles.Should().BeEquivalentTo(["Buyer"]);
    }

    [Fact]
    public void ToSessionUser_FallsBackFromUserNameToEmail()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "fallback@hivespace.local",
            UserName = null,
            EmailConfirmed = true,
            Status = UserStatus.Active
        };

        var sessionUser = Invoke<SessionUser>("ToSessionUser", user, new[] { "Buyer" });

        sessionUser.DisplayName.Should().Be("fallback@hivespace.local");
    }

    [Fact]
    public void ToSessionUser_UsesEmptyStringWhenEmailIsNull()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = null,
            UserName = "named-user",
            EmailConfirmed = false,
            Status = UserStatus.Pending
        };

        var sessionUser = Invoke<SessionUser>("ToSessionUser", user, new[] { "Buyer" });

        sessionUser.Email.Should().Be(string.Empty);
        sessionUser.DisplayName.Should().Be("named-user");
    }

    private static T Invoke<T>(string methodName, params object?[] args)
    {
        var method = FindMethod(methodName, args.Length);
        return (T)method.Invoke(null, args)!;
    }

    private static async Task<T> InvokeTask<T>(string methodName, params object?[] args)
    {
        var method = FindMethod(methodName, args.Length);
        var task = (Task)method.Invoke(null, args)!;
        await task;
        return (T)task.GetType().GetProperty("Result")!.GetValue(task)!;
    }

    private static MethodInfo FindMethod(string methodName, int parameterCount)
        => Subject.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Single(m => m.Name == methodName && m.GetParameters().Length == parameterCount);
}
